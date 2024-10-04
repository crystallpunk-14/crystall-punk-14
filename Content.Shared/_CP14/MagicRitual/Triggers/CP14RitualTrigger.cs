using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicRitual.Triggers;

[ImplicitDataDefinitionForInheritors]
[MeansImplicitUse]
public abstract partial class CP14RitualTrigger
{
    public abstract void Initialize(EntityManager entManager, Entity<CP14MagicRitualComponent> ritual);

    public abstract string? GetGuidebookTriggerDescription(IPrototypeManager prototype, IEntitySystemManager entSys);
}
