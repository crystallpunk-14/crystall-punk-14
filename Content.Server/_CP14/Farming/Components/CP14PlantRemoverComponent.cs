using Robust.Shared.Audio;

namespace Content.Server._CP14.Farming.Components;

/// <summary>
///
/// </summary>
[RegisterComponent, Access(typeof(CP14FarmingSystem))]
public sealed partial class CP14PlantRemoverComponent : Component
{
    /// <summary>
    ///
    /// </summary>
    [DataField]
    public float DoAfter = 1f;

    [DataField]
    public SoundSpecifier Sound = new SoundCollectionSpecifier("CP14Digging");
}
