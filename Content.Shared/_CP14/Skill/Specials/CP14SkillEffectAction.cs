using Content.Shared.Actions;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Skill.Specials;

public sealed partial class CP14SkillEffectAction : CP14SkillEffect
{
    [DataField(required: true)]
    public List<EntProtoId> Actions = new();

    public override void AddSkill(EntityManager entManager, EntityUid target)
    {
        var actionsSystem = entManager.System<SharedActionsSystem>();

        foreach (var action in Actions)
        {
            actionsSystem.AddAction(target, action);
        }
    }

    public override void RemoveSkill(EntityManager entManager, EntityUid target)
    {
        var actionsSystem = entManager.System<SharedActionsSystem>();

        foreach (var (uid, _) in actionsSystem.GetActions(target))
        {
            if (!entManager.TryGetComponent<MetaDataComponent>(uid, out var metaData))
                continue;

            if (metaData.EntityPrototype == null)
                continue;

            if (!Actions.Contains(metaData.EntityPrototype))
                continue;

            actionsSystem.RemoveAction(target, uid);
        }
    }
}
