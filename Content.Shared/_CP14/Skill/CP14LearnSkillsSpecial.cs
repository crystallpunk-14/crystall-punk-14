using Content.Shared._CP14.Skill.Prototypes;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Skill;

public sealed partial class CP14LearnSkillsSpecial : JobSpecial
{
    [DataField]
    public HashSet<ProtoId<CP14SkillPrototype>> Skills { get; private set; } = new();

    public override void AfterEquip(EntityUid mob)
    {
        var entMan = IoCManager.Resolve<IEntityManager>();
        var skillSys = entMan.System<CP14SharedSkillSystem>();

        foreach (var skill in Skills)
        {
            skillSys.TryAddSkill(mob, skill);
        }
    }
}
