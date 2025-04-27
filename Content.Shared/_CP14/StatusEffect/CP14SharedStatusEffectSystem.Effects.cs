using Content.Shared._CP14.MagicSpell.Spells;

namespace Content.Shared._CP14.StatusEffect;

public sealed partial class CP14SharedStatusEffectSystem
{
    private void InitializeEffects()
    {
        SubscribeLocalEvent<CP14StatusEffectAdditionalComponentsComponent, CP14StatusEffectApplied>(AdditionalComponentsApply);
        SubscribeLocalEvent<CP14StatusEffectAdditionalComponentsComponent, CP14StatusEffectRemoved>(AdditionalComponentsRemove);

        SubscribeLocalEvent<CP14StatusEffectApplySpellEffectComponent, CP14StatusEffectApplied>(SpellEffectApply);
        SubscribeLocalEvent<CP14StatusEffectApplySpellEffectComponent, CP14StatusEffectRemoved>(SpellEffectRemove);
    }

    private void UpdateEffects(float frameTime)
    {
        UpdateSpellEffect(frameTime);
    }

    #region CP14StatusEffectAdditionalComponentsComponent
    private void AdditionalComponentsApply(Entity<CP14StatusEffectAdditionalComponentsComponent> ent, ref CP14StatusEffectApplied args)
    {
        EntityManager.AddComponents(args.Target, ent.Comp.Components, ent.Comp.Overridde);
    }

    private void AdditionalComponentsRemove(Entity<CP14StatusEffectAdditionalComponentsComponent> ent, ref CP14StatusEffectRemoved args)
    {
        EntityManager.RemoveComponents(args.Target, ent.Comp.Components);
    }
    #endregion



    #region CP14StatusEffectApplySpellEffectComponent
    private void SpellEffectApply(Entity<CP14StatusEffectApplySpellEffectComponent> ent, ref CP14StatusEffectApplied args)
    {
        foreach (var effect in ent.Comp.StartEffect)
        {
            effect.Effect(EntityManager, new CP14SpellEffectBaseArgs(null, null, args.Target, Transform(args.Target).Coordinates));
        }
    }

    private void SpellEffectRemove(Entity<CP14StatusEffectApplySpellEffectComponent> ent, ref CP14StatusEffectRemoved args)
    {
        foreach (var effect in ent.Comp.EndEffect)
        {
            effect.Effect(EntityManager, new CP14SpellEffectBaseArgs(null, null, args.Target, Transform(args.Target).Coordinates));
        }
    }

    private void UpdateSpellEffect(float frameTime)
    {
        var query = EntityQueryEnumerator<CP14StatusEffectApplySpellEffectComponent, CP14StatusEffectComponent>();
        while (query.MoveNext(out var ent, out var spellEffect, out var statusEffect))
        {
            if (spellEffect.NextUpdateTime > _timing.CurTime)
                continue;

            if (statusEffect.AppliedTo is null)
                continue;

            spellEffect.NextUpdateTime = _timing.CurTime + spellEffect.UpdateFrequency;

            foreach (var effect in spellEffect.UpdateEffect)
            {
                effect.Effect(EntityManager, new CP14SpellEffectBaseArgs(null, null, statusEffect.AppliedTo, Transform(statusEffect.AppliedTo.Value).Coordinates));
            }
        }
    }
    #endregion
}
