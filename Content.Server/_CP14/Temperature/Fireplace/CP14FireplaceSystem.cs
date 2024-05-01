using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Popups;
using Content.Shared._CP14.Temperature;
using Content.Shared.Interaction;
using Content.Shared.Throwing;
using Robust.Server.GameObjects;
using Robust.Shared.Physics.Events;
using Robust.Shared.Timing;

namespace Content.Server._CP14.Temperature.Fireplace;

public sealed partial class CP14FireplaceSystem : EntitySystem
{
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14FireplaceComponent, OnFireChangedEvent>(OnFireChanged);

        SubscribeLocalEvent<CP14FireplaceComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<CP14FireplaceComponent, ThrowHitByEvent>(OnThrowCollide);
        SubscribeLocalEvent<CP14FireplaceComponent, StartCollideEvent>(OnCollide);
    }

    private void OnFireChanged(Entity<CP14FireplaceComponent> fireplace, ref OnFireChangedEvent args)
    {
        if (!TryComp<FlammableComponent>(fireplace, out var flammable))
            return;

        if (args.OnFire)
            flammable.FirestackFade = 0;
    }

    private void OnInteractUsing(Entity<CP14FireplaceComponent> fireplace, ref InteractUsingEvent args)
    {
        if (!fireplace.Comp.CanInsertByHand)
            return;

        if (!TryComp<CP14FireplaceFuelComponent>(args.Used, out var fuel))
            return;

        TryInsertFuel(fireplace, args.Used, fuel);
    }

    private void OnThrowCollide(Entity<CP14FireplaceComponent> fireplace, ref ThrowHitByEvent args)
    {
        if (!fireplace.Comp.CanInsertByThrow)
            return;

        if (!TryComp<CP14FireplaceFuelComponent>(args.Thrown, out var fuel))
            return;

        TryInsertFuel(fireplace, args.Thrown, fuel);
    }

    private void OnCollide(Entity<CP14FireplaceComponent> fireplace, ref StartCollideEvent args)
    {
        if (!fireplace.Comp.CanInsertByCollide)
            return;

        if (!TryComp<CP14FireplaceFuelComponent>(args.OtherEntity, out var fuel))
            return;

        TryInsertFuel(fireplace, args.OtherEntity, fuel);
    }

    private bool TryInsertFuel(Entity<CP14FireplaceComponent> fireplace, EntityUid fuelUid, CP14FireplaceFuelComponent fuel)
    {
        if (fireplace.Comp.CurrentFuel > fireplace.Comp.MaxFuelLimit)
        {
            _popupSystem.PopupEntity("Full", fireplace);
            return false;
        }

        if (!TryComp<FlammableComponent>(fireplace, out var flammable))
            return false;

        fireplace.Comp.CurrentFuel += fuel.Fuel;
        UpdateAppearance(fireplace, fireplace.Comp);
        QueueDel(fuelUid);
        return true;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = AllEntityQuery<CP14FireplaceComponent, FlammableComponent>();
        while (query.MoveNext(out var uid, out var fireplace, out var flammable))
        {
            if (!flammable.OnFire)
                continue;

            if (_timing.CurTime <= fireplace.NextUpdateTime)
                continue;

            fireplace.NextUpdateTime = _timing.CurTime + fireplace.UpdateFrequency;

            if (fireplace.CurrentFuel >= fireplace.FuelDrainingPerUpdate)
            {
                fireplace.CurrentFuel -= fireplace.FuelDrainingPerUpdate;
                UpdateAppearance(uid, fireplace);
                flammable.FirestackFade = fireplace.FireFadeDelta;
            }
            else
            {
                flammable.FirestackFade = -fireplace.FireFadeDelta;
            }
        }
    }

    public void UpdateAppearance(EntityUid uid, CP14FireplaceComponent? fireplace = null, AppearanceComponent? appearance = null)
    {
        if (!Resolve(uid, ref fireplace, ref appearance))
            return;

        if (fireplace.CurrentFuel < fireplace.FuelDrainingPerUpdate)
        {
            _appearance.SetData(uid, FireplaceFuelVisuals.Status, FireplaceFuelStatus.Empty, appearance);
            return;
        }

        if (fireplace.CurrentFuel < fireplace.MaxFuelLimit / 2)
            _appearance.SetData(uid, FireplaceFuelVisuals.Status, FireplaceFuelStatus.Medium, appearance);
        else
            _appearance.SetData(uid, FireplaceFuelVisuals.Status, FireplaceFuelStatus.Full, appearance);
    }
}
