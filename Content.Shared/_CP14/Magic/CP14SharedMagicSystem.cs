using Content.Shared._CP14.Magic.Components;
using Content.Shared._CP14.Magic.Events;
using Content.Shared._CP14.Magic.Events.Actions;
using Content.Shared.DoAfter;
using Content.Shared.EntityEffects;

namespace Content.Shared._CP14.Magic;

/// <summary>
///
/// </summary>
public sealed class CP14SharedMagicSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14MagicEffectComponent, CP14BeforeCastMagicEffectEvent>(OnBeforeCastMagicEffect);

        SubscribeLocalEvent<CP14DelayedEntityEffectActionEvent>(OnEntityEffectAction);
        SubscribeLocalEvent<CP14DelayedMagicEffectComponent, CP14CastMagicEffectDoAfterEvent>(OnMagicEffectDoAfter);
    }

    private void OnEntityEffectAction(CP14DelayedEntityEffectActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        if (args is not ICP14DelayedMagicEffect delayedEffect)
            return;

        var doAfterEventArgs = new DoAfterArgs(EntityManager, args.Performer, delayedEffect.Delay, new CP14CastMagicEffectDoAfterEvent(), args.Action, args.Target)
        {
            BreakOnMove = delayedEffect.BreakOnMove,
            BreakOnDamage = delayedEffect.BreakOnDamage,
        };

        _doAfter.TryStartDoAfter(doAfterEventArgs);
    }

    private void OnMagicEffectDoAfter(Entity<CP14DelayedMagicEffectComponent> ent, ref CP14CastMagicEffectDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Target == null)
            return;

        args.Handled = true;

        foreach (var effect in ent.Comp.Effects)
        {
            effect.Effect(new EntityEffectBaseArgs(args.Target.Value, EntityManager));
        }
    }

    private void OnBeforeCastMagicEffect(Entity<CP14MagicEffectComponent> ent, ref CP14BeforeCastMagicEffectEvent args)
    {
    }
}

