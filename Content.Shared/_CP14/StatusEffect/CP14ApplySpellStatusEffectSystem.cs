using Content.Shared._CP14.MagicSpell.Spells;
using Content.Shared.Damage;
using Content.Shared.StatusEffectNew;
using Content.Shared.StatusEffectNew.Components;
using Robust.Shared.Timing;

namespace Content.Shared._CP14.StatusEffect;

public sealed partial class CP14ApplySpellStatusEffectSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14ApplySpellStatusEffectComponent, StatusEffectAppliedEvent>(SpellEffectApply);
        SubscribeLocalEvent<CP14ApplySpellStatusEffectComponent, StatusEffectRemovedEvent>(SpellEffectRemove);

        SubscribeLocalEvent<CP14DamageModifierStatusEffectComponent, StatusEffectRelayedEvent<DamageModifyEvent>>(OnDamageModify);
    }

    private void OnDamageModify(Entity<CP14DamageModifierStatusEffectComponent> ent, ref StatusEffectRelayedEvent<DamageModifyEvent> args)
    {
        DamageSpecifier newDamage = new();
        foreach (var (type, damage) in args.Args.Damage.DamageDict)
        {
            var dmg = damage * ent.Comp.GlobalDefence;

            if (ent.Comp.Defence is not null && ent.Comp.Defence.TryGetValue(type, out var typeDefence))
                dmg *= typeDefence;

            newDamage.DamageDict[type] = dmg;
        }
        args.Args.Damage = newDamage;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CP14ApplySpellStatusEffectComponent, StatusEffectComponent>();
        while (query.MoveNext(out var ent, out var spellEffect, out var statusEffect))
        {
            if (spellEffect.NextUpdateTime > _timing.CurTime)
                continue;

            if (statusEffect.AppliedTo is null)
                continue;

            spellEffect.NextUpdateTime += spellEffect.UpdateFrequency;

            foreach (var effect in spellEffect.UpdateEffect)
            {
                effect.Effect(EntityManager, new CP14SpellEffectBaseArgs(null, null, statusEffect.AppliedTo, Transform(statusEffect.AppliedTo.Value).Coordinates));
            }
        }
    }

    private void SpellEffectApply(Entity<CP14ApplySpellStatusEffectComponent> ent, ref StatusEffectAppliedEvent args)
    {
        foreach (var effect in ent.Comp.StartEffect)
        {
            effect.Effect(EntityManager, new CP14SpellEffectBaseArgs(null, null, args.Target, Transform(args.Target).Coordinates));
        }
    }

    private void SpellEffectRemove(Entity<CP14ApplySpellStatusEffectComponent> ent, ref StatusEffectRemovedEvent args)
    {
        foreach (var effect in ent.Comp.EndEffect)
        {
            effect.Effect(EntityManager, new CP14SpellEffectBaseArgs(null, null, args.Target, Transform(args.Target).Coordinates));
        }
    }
}
