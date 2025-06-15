
using Content.Shared.Destructible.Thresholds;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.RandomJobs;

[RegisterComponent, Access(typeof(CP14StationRandomJobsSystem))]
public sealed partial class CP14StationRandomJobsComponent : Component
{
    [DataField]
    public List<CP14RandomJobEntry> Entries = new();
}

[Serializable, DataDefinition]
public sealed partial class CP14RandomJobEntry
{
    [DataField(required: true)]
    public List<ProtoId<JobPrototype>> Jobs = default!;

    [DataField(required: true)]
    public MinMax Count = new(1, 1);

    [DataField]
    public float Prob = 1f;
}
