using Content.Shared._CP14.Skills.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Skills.Components;

/// <summary>
/// a list of skills learned by this entity
/// </summary>
[RegisterComponent, Access(typeof(SharedCP14SkillSystem))]
public sealed partial class CP14SkillsStorageComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public List<ProtoId<CP14SkillPrototype>> Skills { get; private set; }= new();
}
