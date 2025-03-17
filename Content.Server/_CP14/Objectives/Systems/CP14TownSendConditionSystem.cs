using Content.Server._CP14.Objectives.Components;
using Content.Shared._CP14.Cargo;
using Content.Shared.Objectives.Components;
using Content.Shared.Objectives.Systems;
using Content.Shared.Stacks;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._CP14.Objectives.Systems;

public sealed class CP14TownSendConditionSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly SharedObjectivesSystem _objectives = default!;

    private EntityQuery<StealTargetComponent> _stealQuery;
    private EntityQuery<StackComponent> _stackQuery;

    public override void Initialize()
    {
        base.Initialize();

        _stealQuery = GetEntityQuery<StealTargetComponent>();
        _stackQuery = GetEntityQuery<StackComponent>();

        SubscribeLocalEvent<CP14TownSendConditionComponent, ObjectiveAfterAssignEvent>(OnAfterAssign);
        SubscribeLocalEvent<CP14TownSendConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);

        SubscribeLocalEvent<BeforeSellEntities>(OnBeforeSell);
    }

    private void OnBeforeSell(BeforeSellEntities ev)
    {
        var query = EntityQueryEnumerator<CP14TownSendConditionComponent>();

        HashSet<EntityUid> removed = new();
        while (query.MoveNext(out var uid, out var condition))
        {
            foreach (var sentEnt in ev.Sent)
            {
                if (condition.CollectionSent >= condition.CollectionSize)
                    continue;

                if (!_stealQuery.TryComp(sentEnt, out var stealTarget))
                    continue;

                if (stealTarget.StealGroup != condition.CollectGroup)
                    continue;

                if (_stackQuery.TryComp(sentEnt, out var stack))
                {
                    condition.CollectionSent += stack.Count;
                }
                else
                {
                    condition.CollectionSent++;
                }

                if (!removed.Contains(sentEnt))
                    removed.Add(sentEnt);
            }
        }

        foreach (var remove in removed)
        {
            ev.Sent.Remove(remove);
            QueueDel(remove);
        }
    }

    //Set the visual, name, icon for the objective.
    private void OnAfterAssign(Entity<CP14TownSendConditionComponent> condition, ref ObjectiveAfterAssignEvent args)
    {
        condition.Comp.CollectionSize = _random.Next(condition.Comp.MinCollectionSize, condition.Comp.MaxCollectionSize);

        var group = _proto.Index(condition.Comp.CollectGroup);

        var title = Loc.GetString(condition.Comp.ObjectiveText, ("itemName",  Loc.GetString(group.Name)), ("count", condition.Comp.CollectionSize));
        var description = Loc.GetString(condition.Comp.ObjectiveDescription, ("itemName", Loc.GetString(group.Name)), ("count", condition.Comp.CollectionSize));

        _metaData.SetEntityName(condition.Owner, title, args.Meta);
        _metaData.SetEntityDescription(condition.Owner, description, args.Meta);
        _objectives.SetIcon(condition.Owner, group.Sprite, args.Objective);
    }

    private void OnGetProgress(Entity<CP14TownSendConditionComponent> condition, ref ObjectiveGetProgressEvent args)
    {
        var result = (float)condition.Comp.CollectionSent / (float)condition.Comp.CollectionSize;
        result = Math.Clamp(result, 0, 1);
        args.Progress = result;
    }
}
