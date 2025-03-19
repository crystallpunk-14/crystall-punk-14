using System.Linq;
using Content.Shared._CP14.MeleeWeapon.Components;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Placeable;
using Content.Shared.Popups;
using Content.Shared.Timing;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Wieldable;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;

namespace Content.Shared._CP14.MeleeWeapon.EntitySystems;

public sealed class CP14SharpeningSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly UseDelaySystem _useDelay = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14SharpenedComponent, GetMeleeDamageEvent>(OnGetMeleeDamage, after: new[] { typeof(SharedWieldableSystem) });
        SubscribeLocalEvent<CP14SharpenedComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<CP14SharpenedComponent, MeleeHitEvent>(OnMeleeHit);

        SubscribeLocalEvent<CP14SharpeningStoneComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<CP14SharpeningStoneComponent, ActivateInWorldEvent>(OnInteract);
    }

    private void OnMeleeHit(Entity<CP14SharpenedComponent> sharpened, ref MeleeHitEvent args)
    {
        if (!args.HitEntities.Any())
            return;

        sharpened.Comp.Sharpness = MathHelper.Clamp(sharpened.Comp.Sharpness - args.BaseDamage.GetTotal().Float() * sharpened.Comp.SharpnessDamageBy1Damage, 0.1f, 1f);
    }

    private void OnInteract(Entity<CP14SharpeningStoneComponent> stone, ref ActivateInWorldEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<ItemPlacerComponent>(stone, out var itemPlacer))
            return;

        if (itemPlacer.PlacedEntities.Count <= 0)
            return;

        foreach (var item in itemPlacer.PlacedEntities)
        {
            if (!TryComp<CP14SharpenedComponent>(item, out var sharpened))
                continue;

            SharpThing(stone, item, sharpened, args.User);
            return;
        }
    }

    private void OnAfterInteract(Entity<CP14SharpeningStoneComponent> stone, ref AfterInteractEvent args)
    {
        if (!args.CanReach || args.Target == null || !TryComp<CP14SharpenedComponent>(args.Target, out var sharpened))
            return;

        if (TryComp<UseDelayComponent>(stone, out var useDelay) && _useDelay.IsDelayed((stone, useDelay)))
            return;

        SharpThing(stone, args.Target.Value, sharpened, args.User);
    }

    private void SharpThing(Entity<CP14SharpeningStoneComponent> stone, EntityUid target, CP14SharpenedComponent component, EntityUid user)
    {
        var ev = new SharpingEvent()
        {
            User = user,
            Target = target,
        };
        RaiseLocalEvent(stone, ev);

        if (!ev.Canceled)
        {
            _audio.PlayPredicted(stone.Comp.SharpeningSound, target, user);

            _damageableSystem.TryChangeDamage(stone, stone.Comp.SelfDamage);
            _damageableSystem.TryChangeDamage(target, stone.Comp.TargetDamage);

            component.Sharpness = MathHelper.Clamp01(component.Sharpness + stone.Comp.SharpnessHeal);

            if (_net.IsServer)
            {
                Spawn("EffectSparks", Transform(target).Coordinates);
                if (component.Sharpness >= 0.99)
                    _popup.PopupEntity(Loc.GetString("sharpening-ready"), target, user);
            }
        }

        _useDelay.TryResetDelay(stone);
    }

    private void OnExamined(Entity<CP14SharpenedComponent> sharpened, ref ExaminedEvent args)
    {

        if (sharpened.Comp.Sharpness > 0.95f)
        {
            args.PushMarkup(Loc.GetString("sharpening-examined-95"));
            return;
        }

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

    private void OnGetMeleeDamage(Entity<CP14SharpenedComponent> sharpened, ref GetMeleeDamageEvent args)
    {
        var slashDamage = args.Damage.DamageDict.GetValueOrDefault("Slash");
        var piercingDamage = args.Damage.DamageDict.GetValueOrDefault("Piercing");

        args.Damage.DamageDict["Slash"] = slashDamage * sharpened.Comp.Sharpness;
        args.Damage.DamageDict["Piercing"] = piercingDamage * sharpened.Comp.Sharpness;
        args.Damage.DamageDict["Blunt"] = (slashDamage + piercingDamage) / 2 * (1f - sharpened.Comp.Sharpness);
    }

}

/// <summary>
/// Caused on a sharpening stone when someone tries to sharpen an object with it
/// </summary>
public sealed class SharpingEvent : EntityEventArgs
{
    public bool Canceled = false;
    public EntityUid User;
    public EntityUid Target;
}
