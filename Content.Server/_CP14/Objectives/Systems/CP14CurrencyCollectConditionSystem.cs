using Content.Server._CP14.Objectives.Components;
using Content.Server.Objectives.Components;
using Content.Shared._CP14.Currency;
using Content.Shared.Interaction;
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
    [Dependency] private readonly CP14SharedCurrencySystem _currency = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;

    private EntityQuery<ContainerManagerComponent> _containerQuery;

    private HashSet<Entity<TransformComponent>> _nearestEnts = new();

    public override void Initialize()
    {
        base.Initialize();

        _containerQuery = GetEntityQuery<ContainerManagerComponent>();

        SubscribeLocalEvent<CP14CurrencyCollectConditionComponent, ObjectiveAfterAssignEvent>(OnCollectAfterAssign);
        SubscribeLocalEvent<CP14CurrencyStoredConditionComponent, ObjectiveAfterAssignEvent>(OnStoredAfterAssign);

        SubscribeLocalEvent<CP14CurrencyCollectConditionComponent, ObjectiveGetProgressEvent>(OnCollectGetProgress);
        SubscribeLocalEvent<CP14CurrencyStoredConditionComponent, ObjectiveGetProgressEvent>(OnStoredGetProgress);
    }

    private void OnCollectAfterAssign(Entity<CP14CurrencyCollectConditionComponent> condition, ref ObjectiveAfterAssignEvent args)
    {
        _metaData.SetEntityName(condition.Owner, Loc.GetString(condition.Comp.ObjectiveText, ("coins", _currency.GetCurrencyPrettyString(condition.Comp.Currency))), args.Meta);
        _metaData.SetEntityDescription(condition.Owner, Loc.GetString(condition.Comp.ObjectiveDescription, ("coins", _currency.GetCurrencyPrettyString(condition.Comp.Currency))), args.Meta);
        _objectives.SetIcon(condition.Owner, condition.Comp.ObjectiveSprite);
    }

    private void OnStoredAfterAssign(Entity<CP14CurrencyStoredConditionComponent> condition, ref ObjectiveAfterAssignEvent args)
    {
        _metaData.SetEntityName(condition.Owner, Loc.GetString(condition.Comp.ObjectiveText, ("coins", _currency.GetCurrencyPrettyString(condition.Comp.Currency))), args.Meta);
        _metaData.SetEntityDescription(condition.Owner, Loc.GetString(condition.Comp.ObjectiveDescription, ("coins", _currency.GetCurrencyPrettyString(condition.Comp.Currency))), args.Meta);
        _objectives.SetIcon(condition.Owner, condition.Comp.ObjectiveSprite);
    }

    private void OnCollectGetProgress(Entity<CP14CurrencyCollectConditionComponent> condition, ref ObjectiveGetProgressEvent args)
    {
        args.Progress = GetProgress(args.Mind, condition);
    }

    private void OnStoredGetProgress(Entity<CP14CurrencyStoredConditionComponent> condition, ref ObjectiveGetProgressEvent args)
    {
        args.Progress = GetStoredProgress(args.Mind, condition);
    }

    private float GetProgress(MindComponent mind, CP14CurrencyCollectConditionComponent condition)
    {
        if (!_containerQuery.TryGetComponent(mind.OwnedEntity, out var currentManager))
            return 0;

        var containerStack = new Stack<ContainerManagerComponent>();
        var count = 0;

        //check pulling object
        if (TryComp<PullerComponent>(mind.OwnedEntity,
                out var pull)) //TO DO: to make the code prettier? don't like the repetition
        {
            var pulledEntity = pull.Pulling;
            if (pulledEntity != null)
            {
                CheckEntity(pulledEntity.Value, ref containerStack, ref count);
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
                    count += _currency.GetTotalCurrency(entity);

                    // if it is a container check its contents
                    if (_containerQuery.TryGetComponent(entity, out var containerManager))
                        containerStack.Push(containerManager);
                }
            }
        } while (containerStack.TryPop(out currentManager));

        var result = count / (float)condition.Currency;
        result = Math.Clamp(result, 0, 1);
        return result;
    }

    private float GetStoredProgress(MindComponent mind, CP14CurrencyStoredConditionComponent condition)
    {
        var containerStack = new Stack<ContainerManagerComponent>();
        var count = 0;

        var areasQuery = AllEntityQuery<StealAreaComponent, TransformComponent>();
        while (areasQuery.MoveNext(out var uid, out var area, out var xform))
        {
            if (!area.Owners.Contains(mind.Owner))
                continue;

            _nearestEnts.Clear();
            _lookup.GetEntitiesInRange(xform.Coordinates, area.Range, _nearestEnts);
            foreach (var ent in _nearestEnts)
            {
                if (!_interaction.InRangeUnobstructed((uid, xform), (ent, ent.Comp), area.Range))
                    continue;

                CheckEntity(ent, ref containerStack, ref count);
            }
        }

        var result = count / (float)condition.Currency;
        result = Math.Clamp(result, 0, 1);
        return result;
    }

    private void CheckEntity(EntityUid entity, ref Stack<ContainerManagerComponent> containerStack, ref int counter)
    {
        // check if this is the item
        counter += _currency.GetTotalCurrency(entity);

        //we don't check the inventories of sentient entity
        if (!TryComp<MindContainerComponent>(entity, out _))
        {
            // if it is a container check its contents
            if (_containerQuery.TryGetComponent(entity, out var containerManager))
                containerStack.Push(containerManager);
        }
    }
}
