using JetBrains.Annotations;
using Robust.Shared.Map;

namespace Content.Shared._CP14.MagicSpell.Interfaces;

[ImplicitDataDefinitionForInheritors]
[MeansImplicitUse]
public abstract partial class CP14SpellEffect
{
    public abstract void Effect(EntityManager entManager, CP14SpellEffectBaseArgs args);
}

public record class CP14SpellEffectBaseArgs
{
    public EntityUid? Target = null;
    public EntityCoordinates? Position = null;

    public CP14SpellEffectBaseArgs(EntityUid? target, EntityCoordinates? position)
    {
        Target = target;
        Position = position;
    }
}
