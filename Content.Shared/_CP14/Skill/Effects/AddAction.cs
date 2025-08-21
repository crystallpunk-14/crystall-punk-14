using Content.Shared._CP14.Skill.Prototypes;
using Content.Shared.Actions;
using Content.Shared.Examine;
using Content.Shared.Mind;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.Skill.Effects;

public sealed partial class AddAction : CP14SkillEffect
{
    [DataField(required: true)]
    public EntProtoId Action;

    public override void AddSkill(IEntityManager entManager, EntityUid target)
    {
        var actionsSystem = entManager.System<SharedActionsSystem>();
        var actionsContainerSys = entManager.System<ActionContainerSystem>();
        var mindSys = entManager.System<SharedMindSystem>();

        if (!mindSys.TryGetMind(target, out var mind, out _))
            actionsSystem.AddAction(target, Action);
        else
            actionsContainerSys.AddAction(mind, Action);
    }

    public override void RemoveSkill(IEntityManager entManager, EntityUid target)
    {
        var actionsSystem = entManager.System<SharedActionsSystem>();
        var actionsContainerSys = entManager.System<ActionContainerSystem>();
        var mindSys = entManager.System<SharedMindSystem>();

        foreach (var (uid, _) in actionsSystem.GetActions(target))
        {
            if (!entManager.TryGetComponent<MetaDataComponent>(uid, out var metaData))
                continue;

            if (metaData.EntityPrototype == null)
                continue;

            if (metaData.EntityPrototype != Action)
                continue;

            if (!mindSys.TryGetMind(target, out var mind, out _))
                actionsSystem.RemoveAction(target, uid);
            else
                actionsContainerSys.RemoveAction(uid);
        }
    }

    public override string? GetName(IEntityManager entMagager, IPrototypeManager protoManager)
    {
        return !protoManager.TryIndex(Action, out var indexedAction) ? string.Empty : indexedAction.Name;
    }

    public override string? GetDescription(IEntityManager entMagager, IPrototypeManager protoManager, ProtoId<CP14SkillPrototype> skill)
    {
        var dummyAction = entMagager.Spawn(Action);
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
