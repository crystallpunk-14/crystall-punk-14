using Content.Shared._CP14.Fishing.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Throwing;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared._CP14.Fishing;

public abstract class CP14SharedFishingSystem : EntitySystem
{
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _userInterface = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    private EntityQuery<CP14FishComponent> _fishQuery;

    public override void Initialize()
    {
        base.Initialize();

        _fishQuery = GetEntityQuery<CP14FishComponent>();

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

            _random.SetSeed((int)_gameTiming.CurTick.Value + GetNetEntity(uid).Id);

            UpdateFishWaitingStatus((uid, fishRod), curTime);
            UpdatePositions((uid, fishRod), curTime);
        }
    }

    /// <summary>
    /// Handles float and fish positions updates
    /// </summary>
    /// <remarks> Please burn it down </remarks>
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

        var fishPos = fishComp.Position;
        var fishDest = fishComp.Destination;
        var fishBaseWaitTime = fishComp.Behavior.BaseWaitTime;

        if (Math.Abs(fishPos - fishDest) < 0.1f)
        {
            UpdateFishDestination((fish.Value, fishComp), curTime, maxCord);
            fishComp.SelectPosTime = curTime + fishBaseWaitTime +  fishBaseWaitTime * 0.2 * _random.NextFloat(-1, 1);
        }
        else
        {
            fishComp.Position = fishComp.Behavior.TryCalculatePosition(_random, fishComp.Position, fishComp.Destination);
        }

        DirtyField(rod, rod.Comp, nameof(CP14FishingRodComponent.FloatPosition));
        DirtyField(fish.Value, fishComp, nameof(CP14FishComponent.Position));
    }

    /// <summary>
    /// Calculates new fish destination
    /// </summary>
    private void UpdateFishDestination(Entity<CP14FishComponent> fish, TimeSpan curTime, float maxCord)
    {
        if (curTime < fish.Comp.SelectPosTime)
            return;

        fish.Comp.Destination = _random.NextFloat(0, maxCord);
        DirtyField(fish, fish.Comp, nameof(CP14FishComponent.Destination));
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

        if (curTime < fishComp.GetAwayTime)
            return;

        rod.Comp.CaughtFish = null;
        DirtyField(rod, rod.Comp, nameof(CP14FishingRodComponent.CaughtFish));
        PredictedDel(fish);
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

    /// <summary>
    /// Sets <see cref="CP14FishingRodComponent.Reeling"/> to user button status
    /// </summary>
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

    /// <summary>
    /// Starts new fishing process when user interacts with pond using fishing rod
    /// </summary>
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

    /// <summary>
    /// Deletes <see cref="CP14FishingRodComponent.User"/> link
    /// </summary>
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
}
