using Content.Shared._CP14.MagicSpell;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Chemistry.SmokeBomb;

/// <summary>
/// Requires the user to play music to use this spell
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedMagicSystem))]
public sealed partial class CP14SmokeBombComponent : Component
{
    [DataField]
    public string Solution = "bomb";

    [DataField]
    public EntProtoId SmokeProto = "CP14Mist";

    [DataField]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/Effects/smoke.ogg");

    [DataField]
    public float Duration = 10;
}
