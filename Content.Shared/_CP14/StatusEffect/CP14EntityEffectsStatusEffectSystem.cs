using Content.Shared._CP14.MagicSpell.Spells;
using Content.Shared.Damage;
using Content.Shared.EntityEffects;
using Content.Shared.StatusEffectNew;
using Content.Shared.StatusEffectNew.Components;
using Robust.Shared.Timing;

namespace Content.Shared._CP14.StatusEffect;

public sealed partial class CP14EntityEffectsStatusEffectSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly EntityManager _entityManager = default!;




    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14EntityEffectsStatusEffectComponent, StatusEffectAppliedEvent>(OnApply);
        SubscribeLocalEvent<CP14EntityEffectsStatusEffectComponent, StatusEffectRemovedEvent>(OnRemove);

    }


    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<CP14EntityEffectsStatusEffectComponent, StatusEffectComponent>();
        while (query.MoveNext(out var ent, out var entityEffect, out var statusEffect))
        {
            if (entityEffect.NextUpdateTime > _timing.CurTime)
                continue;

            if (statusEffect.AppliedTo is not EntityUid targetUid)
                continue;

            entityEffect.NextUpdateTime += entityEffect.Frequency;
            foreach (var effect in entityEffect.Effects)
            {
                //Apply Effect on target
                effect.Effect(new(targetUid, _entityManager));
            }
        }
    }

    private void OnApply(Entity<CP14EntityEffectsStatusEffectComponent> ent, ref StatusEffectAppliedEvent args)
    {
    }

    private void OnRemove(Entity<CP14EntityEffectsStatusEffectComponent> ent, ref StatusEffectRemovedEvent args)
    {

    }
}
