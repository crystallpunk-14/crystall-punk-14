using Content.Shared._CP14.Demiplane.Components;
using Content.Shared._CP14.Demiplane.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Demiplane.Components;

/// <summary>
/// Stores the data needed to generate a new demiplane
/// </summary>
[RegisterComponent, Access(typeof(CP14DemiplaneSystem))]
public sealed partial class CP14DemiplaneGeneratorDataComponent : Component
{
    [DataField]
    public ProtoId<CP14DemiplaneLocationPrototype>? Location;

    [DataField]
    public List<ProtoId<CP14DemiplaneModifierPrototype>> Modifiers = new();

    [DataField]
    public float DifficultyLimit = 1;

    [DataField]
    public float RewardLimit = 1;

    [DataField]
    public int MaxModifiers = 6;
}
