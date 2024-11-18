using Robust.Shared.Audio;

namespace Content.Server._CP14.PersonalSignature;

[RegisterComponent]
public sealed partial class CP14PersonalSignatureComponent : Component
{
    [DataField]
    public SoundSpecifier? SignSound;
}
