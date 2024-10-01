using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.MagicRituals.Components.Requirements;

[ImplicitDataDefinitionForInheritors]
[MeansImplicitUse]
public abstract partial class CP14RitualRequirement
{
    /// <summary>
    /// If this checks fails, the ritual will lose some of its stability.
    /// </summary>
    [DataField]
    public float FailStabilityCost;

    public abstract bool Check(EntityManager entManager, Entity<CP14MagicRitualPhaseComponent> phaseEnt, float stability);

    public abstract string? GetGuidebookRequirementDescription(IPrototypeManager prototype, IEntitySystemManager entSys);
}
