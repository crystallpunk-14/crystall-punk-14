using Robust.Shared.Audio;

namespace Content.Shared._CP14.LockKey.Components;

/// <summary>
/// Allows, when interacting with keys, to mill different teeth, changing the shape of the key
/// </summary>
[RegisterComponent]
public sealed partial class CP14LockRemoverComponent : Component
{
    [DataField]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/_CP14/Items/lockpick_fail.ogg")
    {
        Params = AudioParams.Default
            .WithVariation(0.05f)
            .WithVolume(0.5f),
    };

    [DataField]
    public TimeSpan RemoveTime = TimeSpan.FromSeconds(3.0f);
}
