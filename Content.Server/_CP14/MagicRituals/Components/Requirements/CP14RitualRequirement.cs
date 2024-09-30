using JetBrains.Annotations;

namespace Content.Server._CP14.MagicRituals.Components.Requirements;

[ImplicitDataDefinitionForInheritors]
[MeansImplicitUse]
public abstract partial class CP14RitualRequirement
{
    public abstract bool Check(EntityManager entManager, EntityUid phaseEnt);
}
