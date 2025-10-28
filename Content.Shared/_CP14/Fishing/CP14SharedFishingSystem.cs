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
    [Dependency] private readonly SharedUserInterfaceSystem _userInterface = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly INetManager _net = default!;

    private MapId? _mapId;

    private  EntityQuery<CP14FishComponent> _fishQuery;
    private  EntityQuery<CP14FishingPondComponent> _pondQuery;

    public override void Initialize()
    {
        base.Initialize();

        _fishQuery = GetEntityQuery<CP14FishComponent>();
        _pondQuery = GetEntityQuery<CP14FishingPondComponent>();

        SubscribeLocalEvent<CP14FishingRodComponent, AfterInteractEvent>(OnInteract);
        SubscribeLocalEvent<CP14FishingRodComponent, DroppedEvent>(OnDropEvent);
        SubscribeNetworkEvent<CP14FishingReelKeyMessage>(OnReelingMessage);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _gameTiming.CurTime;
        var query = EntityQueryEnumerator<CP14FishingRodComponent>();

        // Seeding prediction doesnt work
        while (query.MoveNext(out var uid, out var fishRod))
        {
            _random.SetSeed((int)_gameTiming.CurTick.Value + GetNetEntity(uid).Id);
            if (fishRod.User is null)
                continue;

            RevalidateFishing((uid, fishRod));

            if (fishRod.User is null)
                continue;

            if (fishRod.FishingFloat is null)
                continue;

            if (fishRod.Target is null)
                continue;

            var fish = fishRod.CaughtFish;

            if (fishRod.CaughtFish is not null && _fishQuery.TryComp(fish, out var fishComp)) //TODO: remove multiple fish TryComp in next functions
                continue;

            TryToCatchFish((uid, fishRod), curTime);
            UpdateFishWaitingStatus((uid, fishRod), curTime);
            UpdatePositions((uid, fishRod), curTime);
        }
    }

    /// <summary>
    /// Handles float and fish positions updates
    /// Please burn it down
    /// </summary>
    private void UpdatePositions(Entity<CP14FishingRodComponent> rod, TimeSpan curTime)
    {
        if (rod.Comp.CaughtFish is null)
            return;

        if (!rod.Comp.FishHooked)
            return;

        var fish = rod.Comp.CaughtFish;

        if (!_fishQuery.TryComp(fish, out var fishComp))
            return;

        _proto.Resolve(rod.Comp.FishingMinigame, out var minigamePrototype);

        if (minigamePrototype is null)
            return;

        var maxCord = minigamePrototype.FishingMinigameSize;
        var floatSpeed = rod.Comp.FloatSpeed;
        var floatPosition = rod.Comp.FloatPosition;

        if (rod.Comp.Reeling)
        {
            Math.Clamp(floatPosition + floatSpeed, 0, maxCord);
        }
        else
        {
            Math.Clamp(floatPosition - floatSpeed, 0, maxCord);
        }

        var fishPos = fishComp.FishPosAndDestination.X;
        var fishDest = fishComp.FishPosAndDestination.Y;
        var fishBaseWaitTime = fishComp.FishBehavior.BaseWaitTime;

        if (Math.Abs(fishPos - fishDest) < 0.1f)
        {
            UpdateFishDestination((fish.Value, fishComp), curTime, maxCord);
            fishComp.FishSelectPosTime = curTime + fishBaseWaitTime +  fishBaseWaitTime * 0.2 * _random.NextFloat(-1, 1);
        }
        else
        {
            fishComp.FishPosAndDestination = fishComp.FishBehavior.TryCalculatePosition(_random, fishComp.FishPosAndDestination);
        }

        DirtyField(rod, rod.Comp, nameof(CP14FishingRodComponent.FloatPosition));
        DirtyField(fish.Value, fishComp, nameof(CP14FishComponent.FishPosAndDestination));
    }

    /// <summary>
    /// Handles updating fish destination
    /// </summary>
    private void UpdateFishDestination(Entity<CP14FishComponent> fish, TimeSpan curTime, float maxCord)
    {
        if (curTime < fish.Comp.FishSelectPosTime)
            return;

        fish.Comp.FishPosAndDestination.X = _random.NextFloat(0, maxCord);
        DirtyField(fish, fish.Comp, nameof(CP14FishComponent.FishPosAndDestination));
    }

    /// <summary>
    /// Handles if fish got caught or flees
    /// </summary>
    private void UpdateFishWaitingStatus(Entity<CP14FishingRodComponent> rod, TimeSpan curTime)
    {
        if (rod.Comp.CaughtFish is null)
            return;

        if (rod.Comp.FishHooked)
            return;

        if (rod.Comp.User is null)
            return;

        var fish = rod.Comp.CaughtFish;
        if (!_fishQuery.TryComp(fish, out var fishComp))
            return;

        if (rod.Comp.Reeling)
        {
            rod.Comp.FishHooked = true;

            _userInterface.TryOpenUi(rod.Owner, CP14FishingUiKey.Key, rod.Comp.User.Value);

            DirtyField(rod, rod.Comp, nameof(CP14FishingRodComponent.FishHooked));
            return;
        }

        if (curTime < fishComp.FishGetAwayTime)
            return;

        rod.Comp.CaughtFish = null;
        DirtyField(rod, rod.Comp, nameof(CP14FishingRodComponent.CaughtFish));
        PredictedDel(fish);
    }

    /// <summary>
    /// Handles fish catching
    /// </summary>
    private bool TryToCatchFish(Entity<CP14FishingRodComponent> rod, TimeSpan curTime)
    {
        if (!_net.IsServer)
            return false;

        if (rod.Comp.CaughtFish is not null)
            return false;

        if (curTime < rod.Comp.FishingTime)
            return false;

        var pond = rod.Comp.Target;
        if (!_pondQuery.TryComp(pond, out var pondComp))
            return false;

        if (pondComp.LootTable is null)
            return false;

        if (_proto.TryIndex(pondComp.LootTable, out var lootTable))
            return false;

        if (lootTable is null)
            return false;

        var fishes = _entityTable.GetSpawns(lootTable, _random.GetRandom());
        var fishId = fishes.First();

        EnsurePausedMap();
        var fish = PredictedSpawnAtPosition(fishId, new EntityCoordinates(_map.GetMap(_mapId!.Value), Vector2.Zero));

        if (!_player.TryGetSessionByEntity(rod.Comp.User!.Value, out var session))
            return false;

        if (!_fishQuery.TryComp(fish, out var fishComp))
            return false;

        _pvs.AddSessionOverride(fish, session);

        rod.Comp.CaughtFish = fish;
        fishComp.FishGetAwayTime = curTime;
        fishComp.FishGetAwayTime += TimeSpan.FromSeconds(_random.NextDouble(rod.Comp.MinAwaitTime, rod.Comp.MaxAwaitTime));
        DirtyField(rod, rod.Comp, nameof(CP14FishingRodComponent.CaughtFish));
        DirtyField(fish, fishComp, nameof(CP14FishComponent.FishGetAwayTime));

        return true;
    }

    /// <summary>
    /// Validates if user is still in range of fishing float
    /// </summary>
    private void RevalidateFishing(Entity<CP14FishingRodComponent> rod)
    {
        if (rod.Comp.FishingFloat is null)
            return;

        if (_transform.InRange(rod.Owner, rod.Comp.FishingFloat.Value, rod.Comp.MaxFishingDistance * 1.5f))
            return;

        PredictedDel(rod.Comp.FishingFloat);

        rod.Comp.FishHooked =  false;
        rod.Comp.CaughtFish =  null;
        rod.Comp.FishingFloat =  null;
        rod.Comp.Target = null;
        rod.Comp.User = null;

        DirtyFields(rod,
            rod.Comp,
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

    private void OnInteract(Entity<CP14FishingRodComponent> rod, ref AfterInteractEvent args)
    {
        if (args.Handled)
            return;

        if (args.Target is not { Valid: true })
            return;

        if (rod.Comp.FishingFloat is not null)
            return;

        if (!TryComp<CP14FishingPondComponent>(args.Target, out _))
            return;

        if (!_interaction.InRangeUnobstructed(rod.Owner, args.Target.Value, rod.Comp.MaxFishingDistance))
            return;

        args.Handled = true;

        rod.Comp.FishingTime = _gameTiming.CurTime;
        rod.Comp.FishingTime += TimeSpan.FromSeconds(_random.NextDouble(rod.Comp.MinAwaitTime, rod.Comp.MaxAwaitTime));
        rod.Comp.User = args.User;

        DirtyFields(rod, rod.Comp, null, nameof(CP14FishingRodComponent.FishingTime), nameof(CP14FishingRodComponent.User));

        ThrowFishingFloat(rod, args.Target.Value);
    }

    private void OnDropEvent(Entity<CP14FishingRodComponent> rod, ref DroppedEvent args)
    {
        rod.Comp.User = null;
        DirtyField(rod, rod.Comp, nameof(CP14FishingRodComponent.User));
    }

    /// <summary>
    /// Spawns and throws fishing float
    /// </summary>
    private void ThrowFishingFloat(Entity<CP14FishingRodComponent> rod, EntityUid fishingPond)
    {
        var rodCoords = Transform(rod).Coordinates;
        var targetCoords = Transform(fishingPond).Coordinates;

        var fishingFloat = PredictedSpawnAtPosition(rod.Comp.FloatPrototype, rodCoords);

        rod.Comp.FishingFloat = fishingFloat;
        rod.Comp.Target = fishingPond;
        DirtyFields(rod,
            rod.Comp,
            null,
            nameof(CP14FishingRodComponent.FishingFloat),
            nameof(CP14FishingRodComponent.Target));

        _throwing.TryThrow(fishingFloat, targetCoords, rod.Comp.ThrowPower, recoil: false, doSpin: false);
    }

    private void EnsurePausedMap()
    {
        if (!_net.IsServer)
            return;

        if (_map.MapExists(_mapId))
            return;

        var mapUid = _map.CreateMap(out var newMapId);
        _meta.SetEntityName(mapUid, Loc.GetString("fishing-paused-map-name"));
        _mapId = newMapId;
        _map.SetPaused(mapUid, true);
    }
}
