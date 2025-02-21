using Content.Shared.DisplacementMap;

namespace Content.Client._CP14.Displacement;

/// <summary>
/// Simply apply displacements to sprite layers
/// </summary>
[RegisterComponent]
public sealed partial class CP14SimpleDisplacementMapComponent : Component
{
    [DataField]
    public Dictionary<string, DisplacementData> Displacements = new();

    public readonly HashSet<string> RevealedLayers = new();
}

