using Content.Shared._CP14.MagicSpell.Spells;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;

namespace Content.Shared._CP14.Actions;

public abstract partial class CP14SharedActionSystem
{
    private void InitializeModularEffects()
    {
        SubscribeLocalEvent<TransformComponent, CP14ActionStartDoAfterEvent>(OnActionTelegraphy);

        SubscribeLocalEvent<TransformComponent, CP14InstantModularEffectEvent>(OnInstantCast);
        SubscribeLocalEvent<TransformComponent, CP14WorldTargetModularEffectEvent>(OnWorldTargetCast);
        SubscribeLocalEvent<TransformComponent, CP14EntityTargetModularEffectEvent>(OnEntityTargetCast);
    }

    private void OnActionTelegraphy(Entity<TransformComponent> ent, ref CP14ActionStartDoAfterEvent args)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        var performer = GetEntity(args.Performer);
        var action = GetEntity(args.Input.Action);
        var target = GetEntity(args.Input.EntityTarget);
        var targetPosition = GetCoordinates(args.Input.EntityCoordinatesTarget);

        if (!TryComp<ActionComponent>(action, out var actionComp))
            return;

        //Instant
        if (TryComp<InstantActionComponent>(action, out var instant) && instant.Event is CP14InstantModularEffectEvent instantModular)
        {
            var spellArgs = new CP14SpellEffectBaseArgs(performer, actionComp.Container, performer, Transform(performer).Coordinates);

            foreach (var effect in instantModular.TelegraphyEffects)
            {
                effect.Effect(EntityManager, spellArgs);
            }
        }

        //World Target
        if (TryComp<WorldTargetActionComponent>(action, out var worldTarget) && worldTarget.Event is CP14WorldTargetModularEffectEvent worldModular && targetPosition is not null)
        {
            var spellArgs = new CP14SpellEffectBaseArgs(performer, actionComp.Container, null, targetPosition.Value);

            foreach (var effect in worldModular.TelegraphyEffects)
            {
                effect.Effect(EntityManager, spellArgs);
            }
        }

        //Entity Target
        if (TryComp<EntityTargetActionComponent>(action, out var entityTarget) && entityTarget.Event is CP14EntityTargetModularEffectEvent entityModular && target is not null)
        {
            var spellArgs = new CP14SpellEffectBaseArgs(performer, actionComp.Container, target, Transform(target.Value).Coordinates);

            foreach (var effect in entityModular.TelegraphyEffects)
            {
                effect.Effect(EntityManager, spellArgs);
            }
        }
    }

    private void OnInstantCast(Entity<TransformComponent> ent, ref CP14InstantModularEffectEvent args)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        var spellArgs = new CP14SpellEffectBaseArgs(args.Performer, args.Action.Comp.Container, args.Performer, Transform(args.Performer).Coordinates);

        foreach (var effect in args.Effects)
        {
            effect.Effect(EntityManager, spellArgs);
        }

        args.Handled = true;
    }

    private void OnWorldTargetCast(Entity<TransformComponent> ent, ref CP14WorldTargetModularEffectEvent args)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        var spellArgs = new CP14SpellEffectBaseArgs(args.Performer, args.Action.Comp.Container, null, args.Target);

        foreach (var effect in args.Effects)
        {
            effect.Effect(EntityManager, spellArgs);
        }

        args.Handled = true;
    }

    private void OnEntityTargetCast(Entity<TransformComponent> ent, ref CP14EntityTargetModularEffectEvent args)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        var spellArgs = new CP14SpellEffectBaseArgs(args.Performer, args.Action.Comp.Container, args.Target, Transform(args.Target).Coordinates);

        foreach (var effect in args.Effects)
        {
            effect.Effect(EntityManager, spellArgs);
        }

        args.Handled = true;
    }
}

public sealed partial class CP14InstantModularEffectEvent : InstantActionEvent
{
    /// <summary>
    /// Effects that will trigger at the beginning of the cast, before mana is spent. Should have no gameplay importance, just special effects, popups and sounds.
    /// </summary>
    [DataField]
    public List<CP14SpellEffect> TelegraphyEffects = new();

    [DataField]
    public List<CP14SpellEffect> Effects = new();
}

public sealed partial class CP14WorldTargetModularEffectEvent : WorldTargetActionEvent
{
    /// <summary>
    /// Effects that will trigger at the beginning of the cast, before mana is spent. Should have no gameplay importance, just special effects, popups and sounds.
    /// </summary>
    [DataField]
    public List<CP14SpellEffect> TelegraphyEffects = new();

    [DataField]
    public List<CP14SpellEffect> Effects = new();
}

public sealed partial class CP14EntityTargetModularEffectEvent : EntityTargetActionEvent
{
    /// <summary>
    /// Effects that will trigger at the beginning of the cast, before mana is spent. Should have no gameplay importance, just special effects, popups and sounds.
    /// </summary>
    [DataField]
    public List<CP14SpellEffect> TelegraphyEffects = new();

    [DataField]
    public List<CP14SpellEffect> Effects = new();
}
