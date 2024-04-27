using Robust.Shared.Audio;

namespace Content.Shared._CP14.LockKey.Components;

/// <summary>
/// A component of a lock that stores its keyhole shape, complexity, and current state.
/// </summary>
[RegisterComponent]
public sealed partial class CP14LockpickComponent : Component
{
    [DataField]
    public int Health = 3;

    [DataField]
    public SoundSpecifier SuccessSound = new SoundPathSpecifier("/Audio/_CP14/Items/lockpick_use.ogg")
    {
        Params = AudioParams.Default
        .WithVariation(0.05f)
        .WithVolume(0.5f)
    };

    [DataField]
    public SoundSpecifier FailSound = new SoundPathSpecifier("/Audio/_CP14/Items/lockpick_fail.ogg")
    {
        Params = AudioParams.Default
        .WithVariation(0.05f)
        .WithVolume(0.5f)
    };
}
