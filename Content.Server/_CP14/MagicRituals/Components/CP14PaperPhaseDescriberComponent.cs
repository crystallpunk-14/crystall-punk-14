using Content.Shared._CP14.MagicRitual;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.MagicRituals.Components;

/// <summary>
///
/// </summary>
[RegisterComponent, Access(typeof(CP14RitualSystem))]
public sealed partial class CP14PaperPhaseDescriberComponent : Component
{
    [DataField(required: true)]
    public EntProtoId StartPhase = default!;

    [DataField]
    public EntityUid? CurrentPhase = null;

    public List<EntProtoId> SearchHistory = new();

    [DataField]
    public List<EntProtoId> Hyperlinks = new();

    public SoundSpecifier UseSound = new SoundCollectionSpecifier("CP14Book")
    {
        Params = AudioParams.Default
            .WithVariation(0.05f)
            .WithVolume(0.5f),
    };
}

