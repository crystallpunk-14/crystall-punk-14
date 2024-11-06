using Content.Shared._CP14.MagicRitual.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicClothing;

/// <summary>
/// Changes the manacost of spells for the bearer
/// </summary>
[RegisterComponent, Access(typeof(CP14MagicClothingSystem))]
public sealed partial class CP14MagicClothingManacostModifyComponent : Component
{
    [DataField]
    public Dictionary<ProtoId<CP14MagicTypePrototype>, FixedPoint2> Modifiers = new();

    [DataField]
    public FixedPoint2 GlobalModifier = 1f;
}
