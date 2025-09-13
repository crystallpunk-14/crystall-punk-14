using Content.Shared._CP14.MagicSpell.Spells;
using Content.Shared.Actions;

namespace Content.Shared._CP14.Actions;

public abstract partial class CP14SharedActionSystem
{
    private void InitializeModularEffects()
    {
        SubscribeLocalEvent<TransformComponent, CP14InstantModularEffectEvent>(OnInstantCast);
        SubscribeLocalEvent<TransformComponent, CP14WorldTargetModularEffectEvent>(OnWorldTargetCast);
        SubscribeLocalEvent<TransformComponent, CP14EntityTargetModularEffectEvent>(OnEntityTargetCast);
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
