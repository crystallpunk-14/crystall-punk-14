using Content.Server._CP14.Objectives.Components;
using Content.Server.Cargo.Systems;
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
    [Dependency] private readonly PricingSystem _price = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14CurrencyCollectConditionComponent, ObjectiveAfterAssignEvent>(OnCollectAfterAssign);
        SubscribeLocalEvent<CP14CurrencyCollectConditionComponent, ObjectiveGetProgressEvent>(OnCollectGetProgress);
    }

    private void OnCollectAfterAssign(Entity<CP14CurrencyCollectConditionComponent> condition, ref ObjectiveAfterAssignEvent args)
    {
        _metaData.SetEntityName(condition.Owner, Loc.GetString(condition.Comp.ObjectiveText, ("coins", _currency.GetCurrencyPrettyString(condition.Comp.Currency))), args.Meta);
        _metaData.SetEntityDescription(condition.Owner, Loc.GetString(condition.Comp.ObjectiveDescription, ("coins", _currency.GetCurrencyPrettyString(condition.Comp.Currency))), args.Meta);
        _objectives.SetIcon(condition.Owner, condition.Comp.ObjectiveSprite);
    }

    private void OnCollectGetProgress(Entity<CP14CurrencyCollectConditionComponent> condition, ref ObjectiveGetProgressEvent args)
    {
        args.Progress = GetProgress(args.Mind, condition);
    }


    private float GetProgress(MindComponent mind, CP14CurrencyCollectConditionComponent condition)
    {
        double count = 0;

        if (mind.OwnedEntity is null)
            return 0;

        count += _price.GetPrice(mind.OwnedEntity.Value);
        count -= _price.GetPrice(mind.OwnedEntity.Value, false); //We don't want to count the price of the entity itself.

        var result = count / (float)condition.Currency;
        result = Math.Clamp(result, 0, 1);
        return (float)result;
    }
}
