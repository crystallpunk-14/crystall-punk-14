using Content.Shared._CP14.Skills.Prototypes;
using Content.Shared.Roles;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Skills.Components;

/// <summary>
/// a component that can be hung on an entity to immediately teach it some skills
/// </summary>
[UsedImplicitly]
public sealed partial class CP14AddSkillSpecial : JobSpecial
{
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public List<ProtoId<CP14SkillPrototype>> Skills = new();

    public override void AfterEquip(EntityUid mob)
    {
        var entMan = IoCManager.Resolve<IEntityManager>();
        var skillSystem = entMan.System<SharedCP14SkillSystem>();
        foreach (var skill in Skills)
        {
            skillSystem.TryLearnSkill(mob, skill);
        }
    }
}
