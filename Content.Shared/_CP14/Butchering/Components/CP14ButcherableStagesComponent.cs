using Content.Shared.Storage;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Butchering;

/// <summary>
/// Enables staged butchering on an entity.
/// Each stage has its own spawn-list; the entity is deleted only at the final stage.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(CP14SharedButcheringSystem))]
public sealed partial class CP14ButcherableStagesComponent : Component
{
    /// <summary>
    /// Ordered list of stages (0..N-1).
    /// </summary>
    [DataField("stages", required: true)]
    public List<ButcherStage> Stages = new();

    /// <summary>
    /// Current stage index (0-based).
    /// </summary>
    [DataField("currentStage"), AutoNetworkedField]
    [ViewVariables(VVAccess.ReadWrite)]
    public int CurrentStage = 0;

    /// <summary>
    /// Optional popup shown when a mid stage completes.
    /// </summary>
    [DataField("midStagePopup")]
    public LocId? MidStagePopup;

    /// <summary>
    /// Optional popup shown when the final stage completes.
    /// </summary>
    [DataField("finalStagePopup")]
    public LocId? FinalStagePopup;
}

/// <summary>
/// Description of a single staged-butcher step.
/// </summary>
[Serializable, DataDefinition]
public sealed partial class ButcherStage
{
    /// <summary>
    /// What to spawn at this stage. Supports vanilla amount/prob/maxAmount semantics.
    /// </summary>
    [DataField("spawned", required: true)]
    public List<EntitySpawnEntry> Spawned = new();

    /// <summary>
    /// If true, the entity is removed after this stage (final).
    /// </summary>
    [DataField("finalStage")]
    public bool FinalStage = false;

    /// <summary>
    /// Should gibbing occur on the final stage before deletion (if the entity has a body)?
    /// Ignored on non-final stages.
    /// </summary>
    [DataField("gibOnFinal")]
    public bool GibOnFinal = true;
}
