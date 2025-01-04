using Content.Shared._CP14.ModularCraft.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.ModularCraft.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class CP14ModularCraftStartPointComponent : Component
{
    /// <summary>
    /// Starting free slots
    /// </summary>
    [DataField]
    public List<ProtoId<CP14ModularCraftSlotPrototype>> StartSlots = new();

    /// <summary>
    /// Current free slots. May vary depending on the modules delivered
    /// </summary>
    [DataField]
    public List<ProtoId<CP14ModularCraftSlotPrototype>> FreeSlots = new();

    /// <summary>
    /// A list of all installed parts.
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<ProtoId<CP14ModularCraftPartPrototype>> InstalledParts = new();

    /// <summary>
    /// Spawned on disassembling
    /// </summary>
    [DataField]
    public EntProtoId? StartProtoPart;

    /// <summary>
    /// Clentside visual layers from installedParts
    /// </summary>
    public HashSet<string> RevealedLayers = new();
}
