namespace Content.Client._CP14.Farming;

[RegisterComponent]
public sealed partial class CP14PlantVisualsComponent : Component
{
    [DataField]
    public int GrowthSteps = 3;

    [DataField]
    public string? GrowState;

    [DataField]
    public string? GrowUnshadedState;
}

public enum PlantVisualLayers : byte
{
    Base,
    BaseUnshaded,
}
