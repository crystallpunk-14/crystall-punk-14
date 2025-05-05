using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.DemiplaneTraveling;

/// <summary>
/// Component for opening demiplane map UI and interacting with StationDemiplaneMapComponent
/// </summary>
[RegisterComponent]
public sealed partial class CP14DemiplaneNavigationMapComponent : Component
{
    /// <summary>
    /// Extracting coordinates from the demiplane node will create this entity and synchronize its modifiers, location and other parameters.
    /// </summary>
    [DataField]
    public EntProtoId KeyProto = "CP14BaseDemiplaneKey";

    [DataField]
    public SoundSpecifier EjectSound = new SoundPathSpecifier("/Audio/Magic/ethereal_exit.ogg");
}
