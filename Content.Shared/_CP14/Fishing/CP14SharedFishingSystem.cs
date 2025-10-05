using System.Linq;
using System.Numerics;
using Content.Shared._CP14.Fishing.Components;
using Content.Shared.EntityTable;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Throwing;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;
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
    [Dependency] private readonly UserInterfaceSystem _userInterface = default!;
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
        SubscribeNetworkEvent<CP14FishingReelKeyMessage>(OnReelingMessage);
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
        if (_netManager.IsClient && _gameTiming.IsFirstTimePredicted)
            return;

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

            _userInterface.OpenUi(fishingRod, CP14FishingUiKey.Key);

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

        if (fishingRodComponent.CaughtFish is not null)
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

    private void RevalidateFishing(EntityUid fishingRod, CP14FishingRodComponent fishingRodComponent)
    {
        if (fishingRodComponent.FishingFloat is null)
            return;

        if (_transform.InRange(fishingRod, fishingRodComponent.FishingFloat.Value, fishingRodComponent.MaxFishingDistance * 1.5f))
            return;

        PredictedDel(fishingRodComponent.FishingFloat);

        fishingRodComponent.FishHooked =  false;
        fishingRodComponent.CaughtFish =  null;
        fishingRodComponent.FishingFloat =  null;
        fishingRodComponent.Target = null;
        fishingRodComponent.User = null;

        DirtyFields(fishingRod,
            fishingRodComponent,
            null,
            nameof(CP14FishingRodComponent.FishingFloat),
            nameof(CP14FishingRodComponent.Target),
            nameof(CP14FishingRodComponent.User),
            nameof(CP14FishingRodComponent.CaughtFish),
            nameof(CP14FishingRodComponent.FishHooked));
    }

    private void OnReelingMessage(CP14FishingReelKeyMessage msg, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not { } player)
            return;

        if (!_hands.TryGetActiveItem(player, out var activeItem) ||
            !TryComp<CP14FishingRodComponent>(activeItem, out var fishingRodComponent))
            return;

        fishingRodComponent.Reeling = msg.Reeling;
        DirtyField(activeItem.Value, fishingRodComponent, nameof(CP14FishingRodComponent.Reeling));
    }

    private void OnInteract(EntityUid fishingRod, CP14FishingRodComponent fishingRodComponent, AfterInteractEvent args)
    {
        if (args.Handled)
            return;

        if (args.Target is not { Valid: true })
            return;

        if (fishingRodComponent.FishingFloat is not null)
            return;

        if (!TryComp<CP14FishingPondComponent>(args.Target, out _))
            return;

        if (!_interaction.InRangeUnobstructed(fishingRod, args.Target.Value, fishingRodComponent.MaxFishingDistance))
            return;

        args.Handled = true;

        fishingRodComponent.FishingTime = _gameTiming.CurTime;
        fishingRodComponent.FishingTime += TimeSpan.FromSeconds(_random.NextDouble(fishingRodComponent.MinAwaitTime, fishingRodComponent.MaxAwaitTime));
        fishingRodComponent.User = args.User;

        DirtyFields(fishingRod, fishingRodComponent, null, nameof(CP14FishingRodComponent.FishingTime), nameof(CP14FishingRodComponent.User));

        CastFloat(fishingRod, fishingRodComponent, args.Target.Value);
    }

    private void OnDropEvent(EntityUid fishingRod, CP14FishingRodComponent fishingRodComponent, DroppedEvent ev)
    {
        fishingRodComponent.User = null;
        DirtyField(fishingRod, fishingRodComponent, nameof(CP14FishingRodComponent.User));
    }

    private void CastFloat(EntityUid fishingRod, CP14FishingRodComponent fishingRodComponent, EntityUid fishingPond)
    {
        var rodCoords = Transform(fishingRod).Coordinates;
        var targetCoords = Transform(fishingPond).Coordinates;

        var fishingFloat = PredictedSpawnAtPosition(fishingRodComponent.FloatPrototype, rodCoords);

        fishingRodComponent.FishingFloat = fishingFloat;
        fishingRodComponent.Target = fishingPond;
        DirtyFields(fishingRod,
            fishingRodComponent,
            null,
            nameof(CP14FishingRodComponent.FishingFloat),
            nameof(CP14FishingRodComponent.Target));

        _throwing.TryThrow(fishingFloat, targetCoords, fishingRodComponent.ThrowPower, recoil: false, doSpin: false);
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
}
