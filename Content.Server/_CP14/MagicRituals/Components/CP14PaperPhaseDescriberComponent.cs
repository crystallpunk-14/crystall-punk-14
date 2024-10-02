namespace Content.Server._CP14.MagicRituals.Components;

/// <summary>
/// Allows all information about the current phase to be written to the PaperComponent when used in a ritual
/// </summary>
[RegisterComponent, Access(typeof(CP14RitualSystem))]
public sealed partial class CP14PaperPhaseDescriberComponent : Component
{
    [DataField]
    public float DescribeTime = 1.5f;
}

