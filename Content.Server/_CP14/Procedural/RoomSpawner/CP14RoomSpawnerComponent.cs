using Content.Shared.Procedural;
using Content.Shared.Random;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Procedural.RoomSpawner;

/// <summary>
///
/// </summary>
[RegisterComponent, Access(typeof(CP14RoomSpawnerSystem))]
public sealed partial class CP14RoomSpawnerComponent : Component
{
    [DataField(required: true)]
    public ProtoId<WeightedRandomPrototype> RoomsRandom = new();

    [DataField]
    public bool Rotation = true;

    [DataField]
    public bool ClearExisting = true;
}
