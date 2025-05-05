using Content.Shared._CP14.Demiplane.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Demiplane.Components;

/// <summary>
/// Fills the DemiplaneDataComponent with random modifiers and location
/// </summary>
[RegisterComponent, Access(typeof(CP14DemiplaneSystem))]
public sealed partial class CP14DemiplaneRandomGeneratorComponent : Component
{
    [DataField]
    public bool OverrideLocation = false;

    /// <summary>
    /// Demiplane Difficulty Level. By design, the plan so far is for a framework of 1 to 10, but technically could support more.
    /// </summary>
    [DataField(required: true)]
    public int Level = 1;

    [DataField(required: true)]
    public Dictionary<ProtoId<CP14DemiplaneModifierCategoryPrototype>, float> Limits = new();
}
