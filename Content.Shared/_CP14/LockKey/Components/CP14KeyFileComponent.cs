using Robust.Shared.Audio;

namespace Content.Shared._CP14.LockKey.Components;

/// <summary>
/// Allows, when interacting with keys, to mill different teeth, changing the shape of the key
/// </summary>
[RegisterComponent]
public sealed partial class CP14KeyFileComponent : Component
{
    /// <summary>
    /// sound when used
    /// </summary>
    [DataField]
    public SoundSpecifier UseSound =
        new SoundPathSpecifier("/Audio/_CP14/Items/sharpening_stone.ogg")
        {
            Params = AudioParams.Default.WithVariation(0.02f),
        };
}
