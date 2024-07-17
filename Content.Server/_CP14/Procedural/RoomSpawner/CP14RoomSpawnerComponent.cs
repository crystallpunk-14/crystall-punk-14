using Content.Shared.Random;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Procedural.RoomSpawner;

/// <summary>
/// allows you to spawn one of the rooms during initialization
/// </summary>
[RegisterComponent, Access(typeof(CP14RoomSpawnerSystem))]
public sealed partial class CP14RoomSpawnerComponent : Component
{
    [DataField(required: true)]
    public List<ProtoId<TagPrototype>> RoomsTag = new();

    [DataField]
    public float Prob = 1f;

    [DataField]
    public bool Rotation = true;

    [DataField]
    public bool ClearExisting = true;
}
