using Content.Shared.DeviceLinking;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.DeviceLinking.Components;

/// <summary>
/// This component allows the facility to register the weight of objects above it and provide signals to devices
/// </summary>
[RegisterComponent, Access(typeof(CP14PressurePlateSystem))]
public sealed partial class CP14PressurePlateComponent : Component
{
    [DataField]
    public bool IsPressed;

    /// <summary>
    /// The required weight of an object that happens to be above the slab to activate.
    /// </summary>
    [DataField]
    public float WeightRequired = 100f;

    [DataField]
    public float CurrentWeight;

    [DataField]
    public ProtoId<SourcePortPrototype> PressedPort = "CP14Pressed";

    [DataField]
    public ProtoId<SourcePortPrototype> StatusPort = "Status";

    [DataField]
    public ProtoId<SourcePortPrototype> ReleasedPort = "CP14Released";

    [DataField]
    public SoundSpecifier PressedSound = new SoundPathSpecifier("/Audio/Machines/button.ogg");

    [DataField]
    public SoundSpecifier ReleasedSound = new SoundPathSpecifier("/Audio/Machines/button.ogg");
}
