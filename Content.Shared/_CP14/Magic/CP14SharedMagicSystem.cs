using Content.Shared._CP14.Magic.Events;
using Content.Shared._CP14.Magic.Events.Actions;
using Content.Shared.EntityEffects;

namespace Content.Shared._CP14.Magic;

/// <summary>
/// Handles learning and using spells (actions)
/// </summary>
public sealed class CP14SharedMagicSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14MagicEffectComponent, CP14BeforeCastMagicEffectEvent>(OnBeforeCastMagicEffect);

        SubscribeLocalEvent<CP14EntityEffectActionEvent>(OnEntityEffectAction);
    }

    private void OnEntityEffectAction(CP14EntityEffectActionEvent args)
    {
        foreach (var effect in args.Effects)
        {
            effect.Effect(new EntityEffectBaseArgs(args.Target, EntityManager));
        }
    }

    private void OnBeforeCastMagicEffect(Entity<CP14MagicEffectComponent> ent, ref CP14BeforeCastMagicEffectEvent args)
    {
        //Проверки, может ли этот спелл быть создан.

    }
}
