using Content.Server.Hands.Systems;
using Content.Shared._CP14.Fishing;
using Content.Shared._CP14.Fishing.Components;
using Content.Shared.Interaction;
using Content.Shared.Throwing;

namespace Content.Server._CP14.Fishing;

public sealed class CP14FishingSystem : CP14SharedFishingSystem
{
    [Dependency] private readonly SharedInteractionSystem _interactionSystem = default!;
    [Dependency] private readonly ThrowingSystem _throwingSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly HandsSystem _handsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14FishingRodComponent, AfterInteractEvent>(OnInteract);
        SubscribeNetworkEvent<FishingReelKeyMessage>(OnReelingMessage);
    }

    public override void Update(float delta)
    {
        base.Update(delta);

        var query = EntityQueryEnumerator<CP14FishingRodComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (component.FishingFloat is null)
                continue;

            if (_transformSystem.InRange(uid, component.FishingFloat.Value, component.MaxFishingDistance * 1.5f))
                continue;

            PredictedDel(component.FishingFloat);
            component.FishingFloat =  null;
            component.Target = null;
        }
    }

    private void OnReelingMessage(FishingReelKeyMessage msg)
    {
        var held = _handsSystem.GetActiveItem(msg.User);

        if (!TryComp<CP14FishingRodComponent>(held, out var rodComponent))
            return;

        rodComponent.Reeling = msg.Reeling;
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

        if (!_interactionSystem.InRangeUnobstructed(uid, args.Target.Value, component.MaxFishingDistance))
            return;

        args.Handled = true;
        CastFloat(uid, component, args.Target.Value);
    }

    private void CastFloat(EntityUid uid, CP14FishingRodComponent component, EntityUid target)
    {
        var rodCoords = Transform(uid).Coordinates;
        var targetCoords = Transform(target).Coordinates;

        var fishingFloat = PredictedSpawnAtPosition(component.FloatPrototype, rodCoords);

        component.FishingFloat = fishingFloat;
        component.Target = target;

        _throwingSystem.TryThrow(fishingFloat, targetCoords, component.ThrowPower, recoil: false, doSpin: false);
    }
}
