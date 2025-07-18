using Robust.Shared.GameStates;

namespace Content.Shared._CP14.AuraDNA;

/// <summary>
/// A component that stores a “blueprint” of the aura, unique to each mind.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(CP14SharedAuraImprintSystem))]
public sealed partial class CP14AuraImprintComponent : Component
{
    [DataField, AutoNetworkedField]
    public string Imprint = string.Empty;

    [DataField]
    public int ImprintLength = 8;

    [DataField]
    public Color ImprintColor = Color.White;
}
