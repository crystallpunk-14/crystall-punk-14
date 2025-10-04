using System.Linq;
using System.Numerics;
using Content.Shared._CP14.Fishing.Components;
using Content.Shared.EntityTable;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Throwing;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Shared._CP14.Fishing;

public abstract class CP14SharedFishingSystem : EntitySystem
{
    [Dependency] private readonly EntityTableSystem _entityTable = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SharedPvsOverrideSystem _pvs= default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly ISharedPlayerManager _playerManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly INetManager _netManager = default!;

    private MapId? _mapId;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14FishingRodComponent, AfterInteractEvent>(OnInteract);
        SubscribeLocalEvent<CP14FishingRodComponent, DroppedEvent>(OnDropEvent);
        SubscribeNetworkEvent<FishingReelKeyMessage>(OnReelingMessage);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _gameTiming.CurTime;
        var query = EntityQueryEnumerator<CP14FishingRodComponent>();
        _random.SetSeed((int)curTime.Ticks);
        while (query.MoveNext(out var uid, out var component))
        {
            if (component.User is null)
                continue;

            RevalidateFishing(uid, component);
            TryToCatchFish(uid, component, curTime);
            UpdateFishWaitingStatus(uid, component, curTime);
            UpdatePositions(uid, component, curTime);
        }
    }

    private void UpdatePositions(EntityUid fishingRod, CP14FishingRodComponent fishingRodComponent, TimeSpan curTime)
    {
        if (fishingRodComponent.CaughtFish is null)
            return;

        if (!fishingRodComponent.FishHooked)
            return;

        var fish = fishingRodComponent.CaughtFish;

        TryComp(fish, out CP14FishComponent? fishComponent);

        if (fishComponent is null)
            return;

        _prototypeManager.Resolve(fishingRodComponent.FishingMinigame, out var minigamePrototype);

        if (minigamePrototype is null)
            return;

        var maxCord = minigamePrototype.FishingMinigameSize;
        var floatSpeed = fishingRodComponent.FloatSpeed;
        var floatPosition = fishingRodComponent.FloatPosition;

        if (fishingRodComponent.Reeling)
        {
            if (floatSpeed + floatPosition < maxCord)
            {
                fishingRodComponent.FloatPosition +=  fishingRodComponent.FloatSpeed;
            }
            else
            {
                fishingRodComponent.FloatPosition = maxCord;
            }
        }
        else
        {
            if (floatPosition - floatSpeed > 0f)
            {
                fishingRodComponent.FloatPosition -=  floatSpeed;
            }
            else
            {
                fishingRodComponent.FloatPosition = 0;
            }
        }

        var fishPos = fishComponent.FishPosAndDestination.X;
        var fishDest = fishComponent.FishPosAndDestination.Y;
        var fishDiff = fishComponent.FishBehavior.Difficulty;

        if (Math.Abs(fishPos - fishDest) < 0.1f)
        {
            UpdateFishDestination(fish.Value, fishComponent, curTime, maxCord);
        }
        else
        {
            fishComponent.FishPosAndDestination = fishComponent.FishBehavior.TryCalculatePosition(_random, fishComponent.FishPosAndDestination);

            if (Math.Abs(fishPos - fishDest) < 0.1f)
            {
                fishComponent.FishSelectPosTime = curTime + fishDiff + fishDiff * 0.2 * _random.NextFloat(-1, 1);
            }
        }

        DirtyField(fishingRod, fishingRodComponent, nameof(CP14FishingRodComponent.FloatPosition));
        DirtyField(fish.Value, fishComponent, nameof(CP14FishComponent.FishPosAndDestination));
    }

    private void UpdateFishDestination(EntityUid fish, CP14FishComponent fishComponent, TimeSpan curTime, float maxCord)
    {
        if (curTime < fishComponent.FishSelectPosTime)
            return;

        fishComponent.FishPosAndDestination.X = _random.NextFloat(0, maxCord);
    }

    private void UpdateFishWaitingStatus(EntityUid fishingRod, CP14FishingRodComponent fishingRodComponent, TimeSpan curTime)
    {
        if (fishingRodComponent.CaughtFish is null)
            return;

        if (fishingRodComponent.FishHooked)
            return;

        var fish = fishingRodComponent.CaughtFish;
        TryComp(fish, out CP14FishComponent? fishComponent);

        if (fishComponent is null)
            return;

        if (fishingRodComponent.Reeling)
        {
            fishingRodComponent.FishHooked = true;

            var user = fishingRodComponent.User;
            RaiseLocalEvent(user!.Value, new FishingUIStatus(true, fishingRodComponent));

            DirtyField(fishingRod, fishingRodComponent, nameof(CP14FishingRodComponent.FishHooked));
            return;
        }

        if (curTime < fishComponent.FishGetAwayTime)
            return;

        fishingRodComponent.CaughtFish = null;
        DirtyField(fishingRod, fishingRodComponent, nameof(CP14FishingRodComponent.CaughtFish));
        PredictedDel(fish);
    }

    private void TryToCatchFish(EntityUid fishingRod, CP14FishingRodComponent fishingRodComponent, TimeSpan curTime)
    {
        if (!_netManager.IsServer)
            return;

        if (curTime < fishingRodComponent.FishingTime)
            return;

        if (fishingRodComponent.User is null)
            return;

        if (fishingRodComponent.FishingFloat is null)
            return;

        if (fishingRodComponent.Target is null)
            return;

        var pond = fishingRodComponent.Target;
        TryComp(pond, out CP14FishingPondComponent? pondComponent);

        if (pondComponent?.LootTable is null)
            return;

        _prototypeManager.Resolve(pondComponent.LootTable, out var lootTable);

        if (lootTable is null)
            return;

        var fishes = _entityTable.GetSpawns(lootTable, _random.GetRandom());
        var fishId = fishes.First();

        EnsurePausedMap();
        var fish = PredictedSpawnAtPosition(fishId, new EntityCoordinates(_map.GetMap(_mapId!.Value), Vector2.Zero));

        _playerManager.TryGetSessionByEntity(fishingRodComponent.User.Value, out var session);

        if (session is null)
            return;

        _pvs.AddSessionOverride(fish, session);

        TryComp(fish, out CP14FishComponent? fishComponent);

        if (fishComponent is null)
            return;

        fishingRodComponent.CaughtFish = fish;
        fishComponent.FishGetAwayTime = curTime;
        fishComponent.FishGetAwayTime += TimeSpan.FromSeconds(_random.NextDouble(fishingRodComponent.MinAwaitTime, fishingRodComponent.MaxAwaitTime));
        DirtyField(fishingRod, fishingRodComponent, nameof(CP14FishingRodComponent.CaughtFish));
        DirtyField(fish, fishComponent, nameof(CP14FishComponent.FishGetAwayTime));
    }

    private void RevalidateFishing(EntityUid fishingRod, CP14FishingRodComponent component)
    {
        if (component.FishingFloat is null)
            return;

        if (_transform.InRange(fishingRod, component.FishingFloat.Value, component.MaxFishingDistance * 1.5f))
            return;

        PredictedDel(component.FishingFloat);

        component.FishingFloat =  null;
        component.Target = null;
        component.User = null;

        DirtyFields(fishingRod,
            component,
            null,
            nameof(CP14FishingRodComponent.FishingFloat),
            nameof(CP14FishingRodComponent.Target),
            nameof(CP14FishingRodComponent.User));
    }

    private void OnReelingMessage(FishingReelKeyMessage msg, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not { } player)
            return;

        if (!_hands.TryGetActiveItem(player, out var activeItem) ||
            !TryComp<CP14FishingRodComponent>(activeItem, out var fishingRodComponent))
            return;

        fishingRodComponent.Reeling = msg.Reeling;
        DirtyField(activeItem.Value, fishingRodComponent, nameof(CP14FishingRodComponent.Reeling));
    }

    private void OnInteract(EntityUid uid, CP14FishingRodComponent component, AfterInteractEvent args)
    {
        if (args.Handled)
            return;

        if (args.Target is not { Valid: true })
            return;

        if (component.FishingFloat is not null)
            return;

        if (!TryComp<CP14FishingPondComponent>(args.Target, out _))
            return;

        if (!_interaction.InRangeUnobstructed(uid, args.Target.Value, component.MaxFishingDistance))
            return;

        args.Handled = true;

        component.FishingTime = _gameTiming.CurTime;
        component.FishingTime += TimeSpan.FromSeconds(_random.NextDouble(component.MinAwaitTime, component.MaxAwaitTime));
        component.User = args.User;

        DirtyFields(uid, component, null, nameof(CP14FishingRodComponent.FishingTime), nameof(CP14FishingRodComponent.User));

        CastFloat(args.Used, component, args.Target.Value);
    }

    private void OnDropEvent(EntityUid entity, CP14FishingRodComponent component, DroppedEvent ev)
    {
        component.User = null;
        DirtyField(entity, component, nameof(CP14FishingRodComponent.User));
    }

    private void CastFloat(EntityUid uid, CP14FishingRodComponent component, EntityUid target)
    {
        var rodCoords = Transform(uid).Coordinates;
        var targetCoords = Transform(target).Coordinates;

        var fishingFloat = PredictedSpawnAtPosition(component.FloatPrototype, rodCoords);

        component.FishingFloat = fishingFloat;
        component.Target = target;
        component.User = uid;
        DirtyFields(uid,
            component,
            null,
            nameof(CP14FishingRodComponent.FishingFloat),
            nameof(CP14FishingRodComponent.Target),
            nameof(CP14FishingRodComponent.User));

        _throwing.TryThrow(fishingFloat, targetCoords, component.ThrowPower, recoil: false, doSpin: false);
    }

    private void EnsurePausedMap()
    {
        if (!_netManager.IsServer)
            return;

        if (_map.MapExists(_mapId))
            return;

        var mapUid = _map.CreateMap(out var newMapId);
        _meta.SetEntityName(mapUid, Loc.GetString("fishing-paused-map-name"));
        _mapId = newMapId;
        _map.SetPaused(mapUid, true);
    }

    [Serializable, NetSerializable]
    public sealed class FishingReelKeyMessage : EntityEventArgs
    {
        public bool Reeling { get; }

        public FishingReelKeyMessage(bool reeling)
        {
            Reeling = reeling;
        }
    }

    public sealed class FishingUIStatus : EntityEventArgs
    {
        public bool UIStatus { get; }

        public CP14FishingRodComponent Component { get; }

        public FishingUIStatus(bool uiStatus, CP14FishingRodComponent component)
        {
            UIStatus = uiStatus;
            Component = component;
        }
    }
}
