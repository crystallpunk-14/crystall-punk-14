using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.DemiplaneTraveling;

/// <summary>
///
/// </summary>
[RegisterComponent]
public sealed partial class CP14DemiplaneNavigationMapComponent : Component
{
    [DataField]
    public EntProtoId KeyProto = "CP14BaseDemiplaneKey";

    [DataField]
    public SoundSpecifier EjectSound = new SoundPathSpecifier("/Audio/Magic/ethereal_exit.ogg");
}
