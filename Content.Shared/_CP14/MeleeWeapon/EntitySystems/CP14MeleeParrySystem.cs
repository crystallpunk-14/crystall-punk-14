using Content.Shared._CP14.MeleeWeapon.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared._CP14.MeleeWeapon.EntitySystems;

public sealed class CP14MeleeParrySystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14MeleeParryComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnMeleeHit(Entity<CP14MeleeParryComponent> ent, ref MeleeHitEvent args)
    {
        if (args.HitEntities.Count != 1)
            return;

        var target = args.HitEntities[0];

        var activeTargetHand = _hands.GetActiveHand(target);

        if (activeTargetHand?.HeldEntity is null)
            return;

        var parriedWeapon = activeTargetHand.HeldEntity.Value;

        if (!TryComp<MeleeWeaponComponent>(parriedWeapon, out var meleeWeaponItem))
            return;

        if (!meleeWeaponItem.Attacking)
            return;

        _hands.TryDrop(target, parriedWeapon);
        _throwing.TryThrow(parriedWeapon, _random.NextAngle().ToWorldVec(), ent.Comp.ParryPower, target);
        _popup.PopupPredicted( Loc.GetString("cp14-successful-parry"), args.User, args.User);
        //_audio.
    }
}
