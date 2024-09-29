using JetBrains.Annotations;

namespace Content.Shared._CP14.MagicRituals.Requirements;

[ImplicitDataDefinitionForInheritors]
[MeansImplicitUse]
public abstract partial class CP14RitualRequirement
{
    public abstract void Check(EntityManager entManager);
}
