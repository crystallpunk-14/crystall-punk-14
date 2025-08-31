using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Content.Shared.Storage;

namespace Content.Shared._CP14.Butchering;

/// <summary>
/// Component that enables staged butchering: each stage spawns its own loot.
/// The entity is deleted only on the final stage.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class CP14ButcherableStagesComponent : Component
{
    /// <summary>
    /// Stages configuration.
    /// </summary>
    [DataField("stages", required: true)]
    public List<ButcherStage> Stages = new();

    /// <summary>
    /// Current stage index (0-based).
    /// </summary>
    [DataField("currentStage")]
    [ViewVariables(VVAccess.ReadWrite)]
    public int CurrentStage = 0;
}

/// <summary>
/// A single stage of the staged butchering.
/// </summary>
[Serializable]
[DataDefinition]
public sealed partial class ButcherStage
{
    /// <summary>
    /// Items to spawn on this stage (supports amount/prob/maxAmount like vanilla).
    /// </summary>
    [DataField("spawned", required: true)]
    public List<EntitySpawnEntry> Spawned = new();

    /// <summary>
    /// If true, entity is removed after this stage.
    /// </summary>
    [DataField("finalStage")]
    public bool FinalStage = false;
}
