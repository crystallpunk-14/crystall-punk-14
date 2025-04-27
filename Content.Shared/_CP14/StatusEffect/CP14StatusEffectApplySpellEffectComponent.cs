using Content.Shared._CP14.MagicSpell.Spells;
using Robust.Shared.GameStates;

namespace Content.Shared._CP14.StatusEffect;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause]
[Access(typeof(CP14SharedStatusEffectSystem))]
public sealed partial class CP14StatusEffectApplySpellEffectComponent : Component
{
    [DataField(serverOnly: true)]
    public List<CP14SpellEffect> StartEffect = new();

    [DataField(serverOnly: true)]
    public List<CP14SpellEffect> EndEffect = new();

    [DataField(serverOnly: true)]
    public List<CP14SpellEffect> UpdateEffect = new();

    [DataField]
    public TimeSpan UpdateFrequency = TimeSpan.FromSeconds(1f);

    [DataField, AutoPausedField]
    public TimeSpan NextUpdateTime = TimeSpan.Zero;
}
