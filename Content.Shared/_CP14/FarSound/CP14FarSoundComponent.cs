using Robust.Shared.Audio;

namespace Content.Shared._CP14.FarSound;

[RegisterComponent]
public sealed partial class CP14FarSoundComponent : Component
{
    [DataField]
    public SoundSpecifier? CloseSound;

    [DataField]
    public SoundSpecifier? FarSound;

    [DataField]
    public float FarRange = 50f;
}
