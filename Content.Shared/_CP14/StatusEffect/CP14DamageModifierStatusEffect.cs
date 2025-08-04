using Content.Shared.Damage.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.StatusEffect;

[RegisterComponent, NetworkedComponent]
[Access(typeof(CP14ApplySpellStatusEffectSystem))]
public sealed partial class CP14DamageModifierStatusEffectComponent : Component
{
    [DataField]
    public Dictionary<ProtoId<DamageTypePrototype>, float>? Defence = null;

    [DataField]
    public float GlobalDefence = 1f;
}
