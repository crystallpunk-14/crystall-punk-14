using Content.Shared._CP14.Skill.Prototypes;
using Content.Shared.Actions;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Skill.Effects;

public sealed partial class AddAction : CP14SkillEffect
{
    [DataField(required: true)]
    public EntProtoId Action;

    public override void AddSkill(IEntityManager entManager, EntityUid target)
    {
        var actionsSystem = entManager.System<SharedActionsSystem>();

        actionsSystem.AddAction(target, Action);
    }

    public override void RemoveSkill(IEntityManager entManager, EntityUid target)
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

    public override string? GetName(IEntityManager entMagager, IPrototypeManager protoManager)
    {
        return !protoManager.TryIndex(Action, out var indexedAction) ? string.Empty : indexedAction.Name;
    }

    public override string? GetDescription(IEntityManager entMagager, IPrototypeManager protoManager, ProtoId<CP14SkillPrototype> skill)
    {
        return !protoManager.TryIndex(Action, out var indexedAction) ? string.Empty : indexedAction.Description;
    }
}
