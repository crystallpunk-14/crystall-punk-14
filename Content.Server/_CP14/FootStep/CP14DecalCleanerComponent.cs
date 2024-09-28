using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.FootStep;

/// <summary>
///
/// </summary>
[RegisterComponent, Access(typeof(CP14FootprintsSystem))]
public sealed partial class CP14DecalCleanerComponent : Component
{
    [DataField]
    public SoundSpecifier Sound = new SoundCollectionSpecifier("CP14Broom");

    [DataField]
    public EntProtoId? SpawnEffect = "CP14DustEffect";

    [DataField]
    public float Range = 1.5f;
}
