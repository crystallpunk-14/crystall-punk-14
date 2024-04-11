using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Placeable;
using Content.Shared.Timing;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Wieldable;
using Robust.Shared.Audio.Systems;

namespace Content.Server._CP14.MeleeWeapon;

public sealed class CPSharpeningSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly UseDelaySystem _useDelay = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CPSharpenedComponent, GetMeleeDamageEvent>(OnGetMeleeDamage, after: new[] { typeof(WieldableSystem) });
        SubscribeLocalEvent<CPSharpenedComponent, ExaminedEvent>(OnExamined);

        SubscribeLocalEvent<CPSharpeningStoneComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<CPSharpeningStoneComponent, ActivateInWorldEvent>(OnInteract);
    }

    private void OnInteract(Entity<CPSharpeningStoneComponent> stone, ref ActivateInWorldEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<ItemPlacerComponent>(stone, out var itemPlacer))
            return;

        if (itemPlacer.PlacedEntities.Count <= 0)
            return;

        foreach (var item in itemPlacer.PlacedEntities)
        {
            if (!TryComp<CPSharpenedComponent>(item, out var sharpened))
                continue;

            SharpThing(stone, item, sharpened);
            return;
        }
    }

    private void OnAfterInteract(Entity<CPSharpeningStoneComponent> stone, ref AfterInteractEvent args)
    {
        if (!args.CanReach || args.Target == null || !TryComp<CPSharpenedComponent>(args.Target, out var sharpened))
            return;

        if (TryComp<UseDelayComponent>(stone, out var useDelay) && _useDelay.IsDelayed( new Entity<UseDelayComponent>(stone, useDelay)))
            return;

        SharpThing(stone, args.Target.Value, sharpened);
    }

    private void SharpThing(Entity<CPSharpeningStoneComponent> stone, EntityUid target, CPSharpenedComponent component)
    {
        _audio.PlayPvs(stone.Comp.SharpeningSound, target);
        Spawn("EffectSparks", Transform(target).Coordinates);

        _damageableSystem.TryChangeDamage(stone, stone.Comp.SelfDamage);
        _damageableSystem.TryChangeDamage(target, stone.Comp.TargetDamage);

        component.Sharpness = MathHelper.Clamp01(component.Sharpness + stone.Comp.SharpnessHeal);

        _useDelay.TryResetDelay(stone);
    }

    private void OnExamined(Entity<CPSharpenedComponent> sharpened, ref ExaminedEvent args)
    {
        if (sharpened.Comp.Sharpness > 0.75f)
        {
            args.PushMarkup(Loc.GetString("sharpening-examined-75"));
            return;
        }

        if (sharpened.Comp.Sharpness > 0.5f)
        {
            args.PushMarkup(Loc.GetString("sharpening-examined-50"));
            return;
        }
        args.PushMarkup(Loc.GetString("sharpening-examined-25"));
    }

    private void OnGetMeleeDamage(Entity<CPSharpenedComponent> sharpened, ref GetMeleeDamageEvent args)
    {
        args.Damage *= sharpened.Comp.Sharpness;
        sharpened.Comp.Sharpness = MathHelper.Clamp(sharpened.Comp.Sharpness - sharpened.Comp.SharpnessDamageByHit, 0.1f, 1f);
    }
}
