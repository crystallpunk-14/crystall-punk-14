using Content.Shared._CP14.MagicRitual.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicRitual;

/// <summary>
///
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedRitualSystem))]
public sealed partial class CP14MagicRitualOrbComponent : Component
{
    [DataField]
    public Dictionary<ProtoId<CP14MagicTypePrototype>, int> Powers = new();
}
