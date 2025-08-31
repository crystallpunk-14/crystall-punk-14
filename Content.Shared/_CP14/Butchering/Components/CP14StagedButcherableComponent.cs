using Content.Shared._CP14.Butchering.Prototypes;
using Content.Shared.Kitchen;
using Content.Shared.Storage;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Butchering.Components;

/// <summary>
/// Enables staged butchering: every cut advances a stage,
/// spawning configured drops. Only at the final stage the entity is removed.
/// Coexists with vanilla ButcherableComponent; if this exists, staged flow is used.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(CP14SharedStagedButcheringSystem))]
public sealed partial class CP14StagedButcherableComponent : Component
{
    /// <summary>
    /// Ordered list of stages (prototype IDs). Each stage may have its own tool requirement and delay.
    /// </summary>
    [DataField(required: true)]
    public List<ProtoId<CP14ButcherStagePrototype>> Stages = new();

    /// <summary>
    /// Current stage index (0-based). Increments after each successful cut.
    /// When it reaches Stages.Count, butchering is finished.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int CurrentStageIndex = 0;

    /// <summary>
    /// Prevents double do-after or multi-spike overlaps.
    /// </summary>
    [ViewVariables]
    public bool BeingButchered;
}

/// <summary>
/// Which tool/process is valid for the stage.
/// Mirrors vanilla enum to stay upstream-compatible.
/// </summary>
[Serializable, NetSerializable]
public enum CP14ButcheringTool : byte
{
    Knife,
    Spike,
    Gibber
}
