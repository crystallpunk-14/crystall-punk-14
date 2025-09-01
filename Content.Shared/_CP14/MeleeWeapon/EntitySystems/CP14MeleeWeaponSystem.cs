using System.Numerics;
using Content.Shared._CP14.MeleeWeapon.Components;
using Content.Shared.Damage;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Shared._CP14.MeleeWeapon.EntitySystems;

public sealed class CP14MeleeWeaponSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly ThrowingSystem _throw = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CP14MeleeSelfDamageComponent, MeleeHitEvent>(OnMeleeHit);
        SubscribeLocalEvent<CP14BonusDistanceMeleeDamageComponent, MeleeHitEvent>(OnDistanceBonusDamage);
        SubscribeLocalEvent<CP14ComboBonusMeleeDamageComponent, MeleeHitEvent>(OnComboBonusDamage);
        SubscribeLocalEvent<CP14LightMeleeKnockdownComponent, MeleeHitEvent>(OnKnockdownAttack);
    }

    private void OnKnockdownAttack(Entity<CP14LightMeleeKnockdownComponent> ent, ref MeleeHitEvent args)
    {
        if (args.CP14Heavy)
            return;

        foreach (var hit in args.HitEntities)
        {
            _stun.TryKnockdown(hit, ent.Comp.KnockdownTime, true, drop: false);

            // Vector from splitter to item
            var direction = Transform(hit).Coordinates.Position - Transform(args.User).Coordinates.Position;
            if (direction != Vector2.Zero)
            {
                var dir = direction.Normalized() * ent.Comp.ThrowDistance;
                _throw.TryThrow(hit, dir, 3);
            }
        }
    }

    private void OnComboBonusDamage(Entity<CP14ComboBonusMeleeDamageComponent> ent, ref MeleeHitEvent args)
    {
        // Resets combo state
        void Reset()
        {
            ent.Comp.HitEntities.Clear();
            ent.Comp.CurrentHeavyAttacks = 0;
            Dirty(ent);
        }

        // No hits this swing → reset
        if (args.HitEntities.Count == 0)
        {
            Reset();
            return;
        }

        var comp = ent.Comp;

        // Not enough heavy attacks accumulated yet
        if (comp.CurrentHeavyAttacks < comp.HeavyAttackNeed)
        {
            // Light attack before threshold → reset combo
            if (!args.CP14Heavy)
            {
                Reset();
                return;
            }

            // Heavy attack: track overlapping targets across swings
            if (comp.HitEntities.Count == 0)
            {
                // First heavy: initialize the set with current hits
                comp.HitEntities.UnionWith(args.HitEntities);
            }
            else
            {
                // Subsequent heavy: keep only targets hit every time
                comp.HitEntities.IntersectWith(args.HitEntities);

                // Diverged to different targets → reset
                if (comp.HitEntities.Count == 0)
                {
                    Reset();
                    return;
                }
            }

            comp.CurrentHeavyAttacks++;
            Dirty(ent);
            return;
        }

        // Threshold reached: a heavy attack now cancels the combo
        if (args.CP14Heavy)
        {
            Reset();
            return;
        }

        // Light attack after enough heavies → check if it hits any tracked target
        if (comp.HitEntities.Overlaps(args.HitEntities))
        {
            if (_timing.IsFirstTimePredicted)
            {
                _audio.PlayPredicted(comp.Sound, ent, args.User);
                args.BonusDamage += comp.BonusDamage;

                // Visual feedback on every hit entity this swing
                foreach (var hit in args.HitEntities)
                    PredictedSpawnAtPosition(comp.VFX, Transform(hit).Coordinates);
            }
        }

        // Combo always ends after the resolving light attack
        Reset();
    }

    private void OnDistanceBonusDamage(Entity<CP14BonusDistanceMeleeDamageComponent> ent, ref MeleeHitEvent args)
    {
        var critical = true;

        if (args.HitEntities.Count == 0)
            return;

        var userPos = _transform.GetWorldPosition(args.User);
        //Crit only if all targets are at distance
        foreach (var hit in args.HitEntities)
        {
            var targetPos = _transform.GetWorldPosition(hit);

            var distance = (userPos - targetPos).Length();
            if (distance < ent.Comp.MinDistance)
            {
                critical = false;
                break;
            }
        }

        if (!critical)
            return;

        if (!_timing.IsFirstTimePredicted)
            return;

        _audio.PlayPredicted(ent.Comp.Sound, ent, args.User);
        args.BonusDamage += ent.Comp.BonusDamage;

        //Visual effect!
        foreach (var hit in args.HitEntities)
        {
            PredictedSpawnAtPosition(ent.Comp.VFX, Transform(hit).Coordinates);
        }
    }

    private void OnMeleeHit(Entity<CP14MeleeSelfDamageComponent> ent, ref MeleeHitEvent args)
    {
        if (!args.IsHit)
            return;
        if (args.HitEntities.Count == 0)
            return;
        _damageable.TryChangeDamage(ent, ent.Comp.DamageToSelf);
    }
}
