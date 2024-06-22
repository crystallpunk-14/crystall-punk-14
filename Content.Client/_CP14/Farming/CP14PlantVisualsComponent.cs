namespace Content.Client._CP14.Farming;

[RegisterComponent]
public sealed partial class CP14PlantVisualizerComponent : Component
{
    [DataField]
    public int GrowthSteps = 3;

    [DataField]
    public string? GrowState;

    [DataField]
    public string? GrowUnshadedState;

    [DataField]
    public bool ShowZeroStep = false;
}

public enum PlantVisualLayers : byte
{
    Base,
    BaseUnshaded,
}
