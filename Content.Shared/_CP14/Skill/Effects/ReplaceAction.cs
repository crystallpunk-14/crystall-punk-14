using Content.Shared._CP14.Skill.Prototypes;
using Content.Shared.Actions;
using Content.Shared.Examine;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.Skill.Effects;

public sealed partial class ReplaceAction : CP14SkillEffect
{
    [DataField(required: true)]
    public EntProtoId OldAction;

    [DataField(required: true)]
    public EntProtoId NewAction;

    public override void AddSkill(IEntityManager entManager, EntityUid target)
    {
        var actionsSystem = entManager.System<SharedActionsSystem>();

        //Remove old action
        foreach (var (uid, _) in actionsSystem.GetActions(target))
        {
            if (!entManager.TryGetComponent<MetaDataComponent>(uid, out var metaData))
                continue;

            if (metaData.EntityPrototype == null)
                continue;

            if (metaData.EntityPrototype != OldAction)
                continue;

            actionsSystem.RemoveAction(target, uid);
        }

        //Add new one
        actionsSystem.AddAction(target, NewAction);
    }

    public override void RemoveSkill(IEntityManager entManager, EntityUid target)
    {
        var actionsSystem = entManager.System<SharedActionsSystem>();

        //Remove old action
        foreach (var (uid, _) in actionsSystem.GetActions(target))
        {
            if (!entManager.TryGetComponent<MetaDataComponent>(uid, out var metaData))
                continue;

            if (metaData.EntityPrototype == null)
                continue;

            if (metaData.EntityPrototype != NewAction)
                continue;

            actionsSystem.RemoveAction(target, uid);
        }

        //Add new one
        actionsSystem.AddAction(target, OldAction);
    }

    public override string? GetName(IEntityManager entMagager, IPrototypeManager protoManager)
    {
        return !protoManager.TryIndex(NewAction, out var indexedAction) ? string.Empty : indexedAction.Name;
    }

    public override string? GetDescription(IEntityManager entMagager, IPrototypeManager protoManager, ProtoId<CP14SkillPrototype> skill)
    {
        var dummyAction = entMagager.Spawn(NewAction);
        var message = new FormattedMessage();
        if (!entMagager.TryGetComponent<MetaDataComponent>(dummyAction, out var meta))
            return null;

        message.AddText(meta.EntityDescription + "\n");
        var ev = new ExaminedEvent(message, dummyAction, dummyAction, true, true);
        entMagager.EventBus.RaiseLocalEvent(dummyAction, ev);

        entMagager.DeleteEntity(dummyAction);
        return ev.GetTotalMessage().ToMarkup();
    }
}
