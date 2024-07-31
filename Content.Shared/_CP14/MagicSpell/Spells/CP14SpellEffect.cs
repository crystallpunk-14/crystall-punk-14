using JetBrains.Annotations;
using Robust.Shared.Map;

namespace Content.Shared._CP14.MagicSpell.Spells;

[ImplicitDataDefinitionForInheritors]
[MeansImplicitUse]
public abstract partial class CP14SpellEffect
{
    public abstract void Effect(EntityManager entManager, CP14SpellEffectBaseArgs args);
}

public record class CP14SpellEffectBaseArgs
{
    public EntityUid? User;
    public EntityUid? Target;
    public EntityCoordinates? Position;

    public CP14SpellEffectBaseArgs(EntityUid? user, EntityUid? target, EntityCoordinates? position)
    {
        User = user;
        Target = target;
        Position = position;
    }
}
