using Content.Shared._CP14.Demiplan.Components;
using Content.Shared._CP14.Demiplan.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Demiplan.Components;

/// <summary>
/// Stores the data needed to generate a new demiplane
/// </summary>
[RegisterComponent, Access(typeof(CP14DemiplanSystem))]
public sealed partial class CP14DemiplanGeneratorDataComponent : Component
{
    [DataField]
    public ProtoId<CP14DemiplanLocationPrototype> LocationConfig = new();

    [DataField]
    public Entity<CP14DemiplanComponent>? GeneratedMap;

    //Generation settings
}
