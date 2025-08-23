using Content.Shared._CP14.AuraDNA;
using Content.Shared.StatusEffectNew.Components;
using Robust.Shared.GameStates;

namespace Content.Shared._CP14.MagicVision.Components;

/// <summary>
/// Makes you leave random imprints of magical aura instead of the original
/// Use only in conjunction with <see cref="StatusEffectComponent"/>, on the status effect entity.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(CP14SharedAuraImprintSystem))]
public sealed partial class CP14HideMagicAuraStatusEffectComponent : Component
{
    [DataField, AutoNetworkedField]
    public string Imprint = string.Empty;
}
