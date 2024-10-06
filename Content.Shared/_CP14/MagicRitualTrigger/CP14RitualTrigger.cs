using Content.Shared._CP14.MagicRitual;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicRitualTrigger;

[ImplicitDataDefinitionForInheritors]
[MeansImplicitUse]
public abstract partial class CP14RitualTrigger
{
    [DataField]
    public RitualPhaseEdge? Edge = null;

    public abstract void Initialize(EntityManager entManager, Entity<CP14MagicRitualPhaseComponent> ritual, RitualPhaseEdge edge);

    public abstract string? GetGuidebookTriggerDescription(IPrototypeManager prototype, IEntitySystemManager entSys);
}
