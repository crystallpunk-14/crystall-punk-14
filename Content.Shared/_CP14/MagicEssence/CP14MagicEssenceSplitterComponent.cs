using Content.Shared.Whitelist;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicEssence;

/// <summary>
///
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(CP14MagicEssenceSystem))]
public sealed partial class CP14MagicEssenceSplitterComponent : Component
{
    [DataField]
    public EntProtoId ImpactEffect = "CP14EssenceSplitterImpactEffect";

    [DataField]
    public float ThrowForce = 10f;

    [DataField]
    public EntityWhitelist? Whitelist;
}
