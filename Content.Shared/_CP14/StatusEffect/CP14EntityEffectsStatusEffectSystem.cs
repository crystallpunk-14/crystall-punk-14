using Content.Shared._CP14.MagicSpell.Spells;
using Content.Shared.Damage;
using Content.Shared.EntityEffects;
using Content.Shared.StatusEffectNew;
using Content.Shared.StatusEffectNew.Components;
using Robust.Shared.Timing;

namespace Content.Shared._CP14.StatusEffect;
public abstract partial class CP14EntityEffectsStatusEffectSystemShared : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14EntityEffectsStatusEffectComponent, StatusEffectAppliedEvent>(OnApply);
        SubscribeLocalEvent<CP14EntityEffectsStatusEffectComponent, StatusEffectRemovedEvent>(OnRemove);

    }

    private void OnApply(Entity<CP14EntityEffectsStatusEffectComponent> ent, ref StatusEffectAppliedEvent args)
    {

    }

    private void OnRemove(Entity<CP14EntityEffectsStatusEffectComponent> ent, ref StatusEffectRemovedEvent args)
    {

    }
}
