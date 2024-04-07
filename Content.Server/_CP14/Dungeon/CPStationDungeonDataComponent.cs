
using Content.Shared.Procedural;
using Robust.Shared.Prototypes;

/// <summary>
///
/// </summary>
[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class CPStationDungeonDataComponent : Component
{
    [DataField]
    public ProtoId<DungeonConfigPrototype> config = "Haunted";
}
