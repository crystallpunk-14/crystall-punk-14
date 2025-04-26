namespace Content.Shared._CP14.StatusEffect;

public sealed partial class CP14SharedStatusEffectSystem
{
    private void InitializeEffects()
    {
        SubscribeLocalEvent<CP14StatusEffectAdditionalComponentsComponent, CP14StatusEffectApplied>(AdditionalComponentsApply);
        SubscribeLocalEvent<CP14StatusEffectAdditionalComponentsComponent, CP14StatusEffectRemoved>(AdditionalComponentsRemove);
    }

    private void AdditionalComponentsApply(Entity<CP14StatusEffectAdditionalComponentsComponent> ent, ref CP14StatusEffectApplied args)
    {
        if (args.Effect.Comp.AppliedTo is null)
            return;

        EntityManager.AddComponents(args.Effect.Comp.AppliedTo.Value, ent.Comp.Components, ent.Comp.Overridde);
    }

    private void AdditionalComponentsRemove(Entity<CP14StatusEffectAdditionalComponentsComponent> ent, ref CP14StatusEffectRemoved args)
    {
        if (args.Effect.Comp.AppliedTo is null)
            return;

        EntityManager.RemoveComponents(args.Effect.Comp.AppliedTo.Value, ent.Comp.Components);
    }
}
