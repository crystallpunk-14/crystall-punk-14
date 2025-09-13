using Content.Server._CP14.MagicEnergy;
using Content.Server.Atmos.Components;
using Content.Server.Chat.Systems;
using Content.Server.Instruments;
using Content.Shared._CP14.Actions.Components;
using Content.Shared._CP14.MagicEnergy.Components;
using Content.Shared._CP14.MagicSpell;
using Content.Shared._CP14.MagicSpell.Components;
using Content.Shared._CP14.MagicSpell.Events;
using Content.Shared._CP14.MagicSpell.Spells;
using Content.Shared.Actions;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.FixedPoint;
using Content.Shared.Instruments;
using Content.Shared.Projectiles;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Whitelist;
using Robust.Server.GameObjects;
using Robust.Shared.Physics.Events;
using Robust.Shared.Random;

namespace Content.Server._CP14.MagicSpell;

public sealed  class CP14MagicSystem : CP14SharedMagicSystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly CP14MagicEnergySystem _magicEnergy = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14AreaEntityEffectComponent, MapInitEvent>(OnAoEMapInit);

        SubscribeLocalEvent<CP14SpellEffectOnHitComponent, MeleeHitEvent>(OnMeleeHit);
        SubscribeLocalEvent<CP14SpellEffectOnHitComponent, ThrowDoHitEvent>(OnProjectileHit);
        SubscribeLocalEvent<CP14SpellEffectOnCollideComponent, StartCollideEvent>(OnStartCollide);

        SubscribeLocalEvent<CP14MagicEffectCastingVisualComponent, CP14StartCastMagicEffectEvent>(OnSpawnMagicVisualEffect);
        SubscribeLocalEvent<CP14MagicEffectCastingVisualComponent, CP14EndCastMagicEffectEvent>(OnDespawnMagicVisualEffect);
    }

    private void OnStartCollide(Entity<CP14SpellEffectOnCollideComponent> ent, ref StartCollideEvent args)
    {
        if (!_random.Prob(ent.Comp.Prob))
            return;

        if (ent.Comp.Whitelist is not null && !_whitelist.IsValid(ent.Comp.Whitelist, args.OtherEntity))
            return;

        foreach (var effect in ent.Comp.Effects)
        {
            effect.Effect(EntityManager, new CP14SpellEffectBaseArgs(null, ent, args.OtherEntity, Transform(args.OtherEntity).Coordinates));
        }
    }

    private void OnProjectileHit(Entity<CP14SpellEffectOnHitComponent> ent, ref ThrowDoHitEvent args)
    {
        if (!_random.Prob(ent.Comp.Prob))
            return;

        if (ent.Comp.Whitelist is not null && !_whitelist.IsValid(ent.Comp.Whitelist, args.Target))
            return;

        foreach (var effect in ent.Comp.Effects)
        {
            effect.Effect(EntityManager, new CP14SpellEffectBaseArgs(args.Thrown, ent, args.Target, Transform(args.Target).Coordinates));
        }
    }

    private void OnMeleeHit(Entity<CP14SpellEffectOnHitComponent> ent, ref MeleeHitEvent args)
    {
        if (HasComp<PacifiedComponent>(args.User)) //IDK how to check if the user is pacified in a better way
            return;

        if (!args.IsHit)
            return;

        if (!_random.Prob(ent.Comp.Prob))
            return;

        foreach (var entity in args.HitEntities)
        {
            if (ent.Comp.Whitelist is not null && !_whitelist.IsValid(ent.Comp.Whitelist, entity))
                continue;

            foreach (var effect in ent.Comp.Effects)
            {
                effect.Effect(EntityManager, new CP14SpellEffectBaseArgs(args.User, ent, entity, Transform(entity).Coordinates));
            }
        }
    }

    private void OnAoEMapInit(Entity<CP14AreaEntityEffectComponent> ent, ref MapInitEvent args)
    {
        var entitiesAround = _lookup.GetEntitiesInRange(ent, ent.Comp.Range, LookupFlags.Uncontained);

        var count = 0;
        foreach (var entity in entitiesAround)
        {
            if (ent.Comp.Whitelist is not null && !_whitelist.IsValid(ent.Comp.Whitelist, entity))
                continue;

            foreach (var effect in ent.Comp.Effects)
            {
                effect.Effect(EntityManager, new CP14SpellEffectBaseArgs(ent, null, entity, Transform(entity).Coordinates));
            }

            count++;

            if (ent.Comp.MaxTargets > 0 && count >= ent.Comp.MaxTargets)
                break;
        }
    }

    private void OnSpawnMagicVisualEffect(Entity<CP14MagicEffectCastingVisualComponent> ent, ref CP14StartCastMagicEffectEvent args)
    {
        var vfx = SpawnAttachedTo(ent.Comp.Proto, Transform(args.Performer).Coordinates);
        _transform.SetParent(vfx, args.Performer);
        ent.Comp.SpawnedEntity = vfx;
    }

    private void OnDespawnMagicVisualEffect(Entity<CP14MagicEffectCastingVisualComponent> ent, ref CP14EndCastMagicEffectEvent args)
    {
        QueueDel(ent.Comp.SpawnedEntity);
        ent.Comp.SpawnedEntity = null;
    }
}
