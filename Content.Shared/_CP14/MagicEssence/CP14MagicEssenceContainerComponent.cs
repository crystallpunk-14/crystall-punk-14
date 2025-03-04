using Content.Shared._CP14.MagicRitual.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicEssence;

/// <summary>
/// Reflects the amount of essence stored in this item. The item can be destroyed to release the essence from it.
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(CP14MagicEssenceSystem))]
public sealed partial class CP14MagicEssenceContainerComponent : Component
{
    [DataField]
    public Dictionary<ProtoId<CP14MagicTypePrototype>, int> Essences = new();
}
