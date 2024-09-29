using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Footprints.Components;

/// <summary>
/// allows you to remove cleanable decals from tiles with a short delay.
/// </summary>
[RegisterComponent, Access(typeof(CP14FootprintsSystem))]
public sealed partial class CP14DecalCleanerComponent : Component
{
    [DataField]
    public SoundSpecifier Sound = new SoundCollectionSpecifier("CP14Broom")
    {
        Params = AudioParams.Default.WithVariation(0.2f),
    };

    [DataField]
    public EntProtoId? SpawnEffect = "CP14DustEffect";

    [DataField]
    public float Range = 1.2f;

    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(0.75f);
}
