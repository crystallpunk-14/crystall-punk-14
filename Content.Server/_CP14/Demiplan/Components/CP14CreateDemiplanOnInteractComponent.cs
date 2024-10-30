using Content.Shared._CP14.Expeditions.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Demiplan.Components;

/// <summary>
/// Creates a demiplane, and the entity associated with it
/// </summary>
[RegisterComponent, Access(typeof(CP14DemiplanSystem))]
public sealed partial class CP14CreateDemiplanOnInteractComponent : Component
{
    [DataField]
    public ProtoId<CP14DemiplanLocationPrototype> LocationConfig = new();
}
