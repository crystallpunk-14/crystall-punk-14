using Content.Shared._CP14.Expeditions.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Expeditions.Components;

/// <summary>
///
/// </summary>
[RegisterComponent, Access(typeof(CP14ExpeditionSystem))]
public sealed partial class CP14ExpeditionGeneratorComponent : Component
{
    //Generated on mapinit
    [DataField]
    public ProtoId<CP14ExpeditionLocationPrototype> DungeonConfig = new();
}
