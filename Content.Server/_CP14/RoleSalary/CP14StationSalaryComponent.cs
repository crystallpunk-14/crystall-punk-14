using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.RoleSalary;

[RegisterComponent, Access(typeof(CP14SalarySystem)), AutoGenerateComponentPause]
public sealed partial class CP14StationSalaryComponent : Component
{
    /// <summary>
    /// listing all the departments and their salaries
    /// </summary>
    [DataField]
    public Dictionary<ProtoId<DepartmentPrototype>, int> Salary = new();

    [DataField, AutoPausedField]
    public TimeSpan NextSalaryTime = TimeSpan.Zero;

    [DataField]
    public TimeSpan SalaryFrequency = TimeSpan.FromMinutes(18);

    [DataField]
    public EntProtoId SalaryProto = "CP14SalarySpawner";
}
