using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicRitual.Actions;

[ImplicitDataDefinitionForInheritors]
[MeansImplicitUse]
public abstract partial class CP14RitualAction
{
    /// <summary>
    /// Effect appearing in place of interacted entities
    /// </summary>
    [DataField("vfx")]
    public EntProtoId? VisualEffect = "CP14DustEffect";

    public abstract void Effect(EntityManager entManager, SharedTransformSystem transform, Entity<CP14MagicRitualPhaseComponent> phase);

    public abstract string? GetGuidebookEffectDescription(IPrototypeManager prototype, IEntitySystemManager entSys);
}
