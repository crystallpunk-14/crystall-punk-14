using Content.Shared.Actions;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Skill.Specials;

public sealed partial class AddAction : CP14SkillEffect
{
    [DataField(required: true)]
    public EntProtoId Action;

    public override void AddSkill(EntityManager entManager, EntityUid target)
    {
        var actionsSystem = entManager.System<SharedActionsSystem>();

        actionsSystem.AddAction(target, Action);
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

            if (metaData.EntityPrototype != Action)
                continue;

            actionsSystem.RemoveAction(target, uid);
        }
    }

    public override string? GetName(EntityManager entMagager, IPrototypeManager protoManager)
    {
        if (!protoManager.TryIndex(Action, out var indexedAction))
            return String.Empty;

        return indexedAction.Name;
    }

    public override string? GetDescription(EntityManager entMagager, IPrototypeManager protoManager)
    {
        if (!protoManager.TryIndex(Action, out var indexedAction))
            return String.Empty;

        return indexedAction.Description;
    }
}
