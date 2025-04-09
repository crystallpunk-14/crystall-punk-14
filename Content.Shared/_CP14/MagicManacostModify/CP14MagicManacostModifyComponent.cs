using Content.Shared._CP14.MagicRitual.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicManacostModify;

/// <summary>
/// Changes the manacost of spells for the bearer
/// </summary>
[RegisterComponent]
public sealed partial class CP14MagicManacostModifyComponent : Component
{
    [DataField]
    public Dictionary<ProtoId<CP14MagicTypePrototype>, FixedPoint2> Modifiers = new();

    [DataField]
    public FixedPoint2 GlobalModifier = 1f;
}
