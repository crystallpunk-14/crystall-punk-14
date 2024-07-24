using Content.Server._CP14.Objectives.Components;
using Content.Shared._CP14.Currency;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Objectives.Components;
using Content.Shared.Objectives.Systems;
using Robust.Shared.Containers;

namespace Content.Server._CP14.Objectives.Systems;

public sealed class CP14CurrencyCollectConditionSystem : EntitySystem
{
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly SharedObjectivesSystem _objectives = default!;
    [Dependency] private readonly CP14CurrencySystem _currency = default!;

    private EntityQuery<ContainerManagerComponent> _containerQuery;

    public override void Initialize()
    {
        base.Initialize();

        _containerQuery = GetEntityQuery<ContainerManagerComponent>();

        SubscribeLocalEvent<CP14CurrencyCollectConditionComponent, ObjectiveAssignedEvent>(OnAssigned);
        SubscribeLocalEvent<CP14CurrencyCollectConditionComponent, ObjectiveAfterAssignEvent>(OnAfterAssign);
        SubscribeLocalEvent<CP14CurrencyCollectConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);
    }

    private void OnAssigned(Entity<CP14CurrencyCollectConditionComponent> condition, ref ObjectiveAssignedEvent args)
    {
    }

    private void OnAfterAssign(Entity<CP14CurrencyCollectConditionComponent> condition, ref ObjectiveAfterAssignEvent args)
    {
        _metaData.SetEntityName(condition.Owner, Loc.GetString(condition.Comp.ObjectiveText), args.Meta);
        _metaData.SetEntityDescription(condition.Owner, Loc.GetString(condition.Comp.ObjectiveDescription, ("coins", _currency.GetPrettyCurrency(condition.Comp.Currency))), args.Meta);
        _objectives.SetIcon(condition.Owner, condition.Comp.ObjectiveSprite);
    }

    private void OnGetProgress(Entity<CP14CurrencyCollectConditionComponent> condition, ref ObjectiveGetProgressEvent args)
    {
        args.Progress = GetProgress(args.Mind, condition);
    }

    private float GetProgress(MindComponent mind, CP14CurrencyCollectConditionComponent condition)
    {
        if (!_containerQuery.TryGetComponent(mind.OwnedEntity, out var currentManager))
            return 0;

        var containerStack = new Stack<ContainerManagerComponent>();
        var count = 0;

        //check pulling object
        if (TryComp<PullerComponent>(mind.OwnedEntity, out var pull)) //TO DO: to make the code prettier? don't like the repetition
        {
            var pulledEntity = pull.Pulling;
            if (pulledEntity != null)
            {
                CheckEntity(pulledEntity.Value, condition, ref containerStack, ref count);
            }
        }

        // recursively check each container for the item
        // checks inventory, bag, implants, etc.
        do
        {
            foreach (var container in currentManager.Containers.Values)
            {
                foreach (var entity in container.ContainedEntities)
                {
                    // check if this is the item
                    count += CheckCurrency(entity, condition);

                    // if it is a container check its contents
                    if (_containerQuery.TryGetComponent(entity, out var containerManager))
                        containerStack.Push(containerManager);
                }
            }
        } while (containerStack.TryPop(out currentManager));

        var result = count / (float) condition.Currency;
        result = Math.Clamp(result, 0, 1);
        return result;
    }

    private void CheckEntity(EntityUid entity, CP14CurrencyCollectConditionComponent condition, ref Stack<ContainerManagerComponent> containerStack, ref int counter)
    {
        // check if this is the item
        counter += CheckCurrency(entity, condition);

        //we don't check the inventories of sentient entity
        if (!TryComp<MindContainerComponent>(entity, out _))
        {
            // if it is a container check its contents
            if (_containerQuery.TryGetComponent(entity, out var containerManager))
                containerStack.Push(containerManager);
        }
    }

    private int CheckCurrency(EntityUid entity, CP14CurrencyCollectConditionComponent condition)
    {
        // check if this is the target
        if (!TryComp<CP14CurrencyComponent>(entity, out var target))
            return 0;

        if (target.Category != condition.Category)
            return 0;

        return _currency.GetTotalCurrency(entity);
    }
}
