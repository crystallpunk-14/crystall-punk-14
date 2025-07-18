using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Dash;

/// <summary>
/// This component marks entities that are currently in the dash
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(CP14DashSystem))]
public sealed partial class CP14DashComponent : Component
{
    [DataField]
    public EntProtoId DashEffect = "CP14DustEffect";

    [DataField]
    public SoundSpecifier DashSound = new SoundPathSpecifier("/Audio/_CP14/Effects/dash.ogg")
    {
        Params = AudioParams.Default.WithVariation(0.05f)
    };
}
