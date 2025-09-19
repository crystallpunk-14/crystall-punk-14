using System.Linq;
using System.Numerics;
using Content.Shared._CP14.Fishing.Components;
using Content.Shared.EntityTable;
using Content.Shared.Interaction.Events;
using Content.Shared.Item;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Shared._CP14.Fishing;

public abstract class CP14SharedFishingSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly EntityTableSystem _entityTable = default!;
    [Dependency] private readonly MetaDataSystem _metaSystem = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;

    private MapId? _mapId;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14FishingRodComponent, GettingPickedUpAttemptEvent>(OnPickupEvent);
        SubscribeLocalEvent<CP14FishingRodComponent, DroppedEvent>(OnDropEvent);
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

            if (component.CaughtFish is null)
            {
                TryToCatchFish(uid, component, curTime);
                continue;
            }

            if (!component.FishHooked)
            {
                UpdateFishWaitingStatus(uid, component, curTime);
                continue;
            }

            UpdatePositions(uid, component, curTime);
        }
    }

    private void UpdatePositions(EntityUid fishingRod, CP14FishingRodComponent fishingRodComponent, TimeSpan curTime)
    {
        var fish = fishingRodComponent.CaughtFish;

        if (fish is null)
            return;

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

        DirtyField(fishingRod, fishingRodComponent, nameof(fishingRodComponent.FloatPosition));
        DirtyField(fish.Value, fishComponent, nameof(fishComponent.FishPosAndDestination));
    }

    private void UpdateFishDestination(EntityUid fish, CP14FishComponent fishComponent, TimeSpan curTime, float maxCord)
    {
        if (curTime < fishComponent.FishSelectPosTime)
            return;

        fishComponent.FishPosAndDestination.X = _random.NextFloat(0, maxCord);
    }

    private void UpdateFishWaitingStatus(EntityUid fishingRod, CP14FishingRodComponent fishingRodComponent, TimeSpan curTime)
    {
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
        Del(fish);
    }

    private void TryToCatchFish(EntityUid fishingRod, CP14FishingRodComponent fishingRodComponent, TimeSpan curTime)
    {
        if (curTime < fishingRodComponent.FishingTime)
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
        var fish = Spawn(fishId, new MapCoordinates(Vector2.Zero, _mapId!.Value));

        TryComp(fish, out CP14FishComponent? fishComponent);

        if (fishComponent is null)
            return;

        fishingRodComponent.CaughtFish = fish;
        fishComponent.FishGetAwayTime = curTime;
        fishComponent.FishGetAwayTime += TimeSpan.FromSeconds(_random.NextDouble(fishingRodComponent.MinAwaitTime, fishingRodComponent.MaxAwaitTime));
        DirtyField(fishingRod, fishingRodComponent, nameof(CP14FishingRodComponent.CaughtFish));
        DirtyField(fish, fishComponent, nameof(CP14FishComponent.FishGetAwayTime));
    }

    private void OnPickupEvent(EntityUid entity, CP14FishingRodComponent component, GettingPickedUpAttemptEvent ev)
    {
        if (ev.Cancelled)
            return;

        component.User = ev.User;
    }

    private void OnDropEvent(EntityUid entity, CP14FishingRodComponent component, DroppedEvent ev)
    {
        component.User = null;
    }

    private void EnsurePausedMap()
    {
        if (_map.MapExists(_mapId))
            return;

        var mapUid = _map.CreateMap(out var newMapId);
        _metaSystem.SetEntityName(mapUid, Loc.GetString("fishing-paused-map-name"));
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
