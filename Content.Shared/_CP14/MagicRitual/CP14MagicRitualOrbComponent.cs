using Content.Shared._CP14.MagicRitual.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicRitual;

/// <summary>
/// “Key” in the concept of rituals. An entity that can be a key to a ritual, and holds certain characteristics that can be spent, or by which a phase transition requirement check can be made.
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedRitualSystem))]
public sealed partial class CP14MagicRitualOrbComponent : Component
{
    [DataField]
    public Dictionary<ProtoId<CP14MagicTypePrototype>, int> Powers = new();
}
