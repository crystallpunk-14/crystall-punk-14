using Content.Server._CP14.PVS;
using Robust.Shared.Prototypes;

namespace MyNamespace;

/// <summary>
/// A spawner that clearly controls how many entities it can spawn.
/// </summary>
[RegisterComponent, Access(typeof(CP14ConstrainSpawnerSystem))]
public sealed partial class CP14ConstrainedSpawnerOnTriggerComponent : Component
{
    [DataField]
    public List<EntProtoId> Prototypes { get; set; } = new();

    public HashSet<EntityUid> Spawned = new();

    [DataField]
    public float Chance { get; set; } = 1.0f;

    [DataField]
    public float Offset { get; set; } = 0.2f;

    [DataField]
    public int MaxCount = 1;
}
