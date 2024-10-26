using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.StealArea;

/// <summary>
///
/// </summary>
[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class CP14StealAreaAutoJobConnectComponent : Component
{
    [DataField]
    public HashSet<ProtoId<JobPrototype>> Jobs = new();

    [DataField]
    public HashSet<ProtoId<DepartmentPrototype>> Departments = new();
}
