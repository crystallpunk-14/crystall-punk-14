using Content.Shared._CP14.Procedural.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Procedural.GlobalWorld.Components;

/// <summary>
/// Generates the surrounding procedural world on the game map, surrounding the mapped settlement.
/// </summary>
[RegisterComponent, Access(typeof(CP14GlobalWorldSystem))]
public sealed partial class CP14StationGlobalWorldIntegrationComponent : Component
{
    [DataField(required: true)]
    public ProtoId<CP14ProceduralLocationPrototype> Location;

    [DataField]
    public List<ProtoId<CP14ProceduralModifierPrototype>> Modifiers = [];

    [DataField]
    public Vector2i GenerationOffset = Vector2i.Zero;
}
