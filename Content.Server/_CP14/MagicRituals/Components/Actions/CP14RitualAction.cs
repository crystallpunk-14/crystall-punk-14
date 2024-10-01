using JetBrains.Annotations;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.MagicRituals.Components.Actions;

[ImplicitDataDefinitionForInheritors]
[MeansImplicitUse]
public abstract partial class CP14RitualAction
{
    /// <summary>
    /// Effect appearing in place of interacted entities
    /// </summary>
    [DataField("vfx")]
    public EntProtoId? VisualEffect = "CP14DustEffect";

    public abstract void Effect(EntityManager entManager, TransformSystem _transform, Entity<CP14MagicRitualPhaseComponent> phase);
}
