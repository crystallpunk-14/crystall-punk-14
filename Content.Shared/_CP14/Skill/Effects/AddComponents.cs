
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Skill.Effects;

public sealed partial class AddComponents : CP14SkillEffect
{
    [DataField(required: true)]
    public ComponentRegistry Components = new();

    public override void AddSkill(IEntityManager entManager, EntityUid target)
    {
        entManager.AddComponents(target, Components);
    }

    public override void RemoveSkill(IEntityManager entManager, EntityUid target)
    {
        entManager.RemoveComponents(target, Components);
    }

    public override string? GetName(IEntityManager entMagager, IPrototypeManager protoManager)
    {
        return null;
    }

    public override string? GetDescription(IEntityManager entMagager, IPrototypeManager protoManager)
    {
        return null;
    }
}
