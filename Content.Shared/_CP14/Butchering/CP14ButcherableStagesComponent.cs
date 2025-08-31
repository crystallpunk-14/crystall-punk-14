using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Content.Shared.Kitchen;
using Content.Shared.Storage;
using Content.Shared._CP14.Butchering;

/// <summary>
/// Allows defining staged butchering for entities.
/// Each stage can spawn different loot, and entity is deleted only on the final stage.
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(CP14ButcheringShared))]
public sealed partial class CP14ButcherableStagesComponent : Component
{
    /// <summary>
    /// List of stages for butchering. Each stage defines items to spawn.
    /// </summary>
    [DataField("stages", required: true)]
    public List<ButcherStage> Stages = new();

    /// <summary>
    /// Current progress of butchering (stage index).
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public int CurrentStage = 0;
}

/// <summary>
/// Defines a single stage of staged butchering.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class ButcherStage
{
    /// <summary>
    /// What items should spawn at this stage.
    /// </summary>
    [DataField("spawned", required: true)]
    public List<EntitySpawnEntry> Spawned = new();

    /// <summary>
    /// If true, the entity will be deleted after this stage.
    /// </summary>
    [DataField("finalStage")]
    public bool FinalStage = false;
}
