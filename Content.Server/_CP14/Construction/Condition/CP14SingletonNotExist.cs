using Content.Shared._CP14.UniqueLoot;
using Content.Shared._CP14.Vampire;
using Content.Shared._CP14.Vampire.Components;
using Content.Shared.Construction;
using Content.Shared.Examine;
using Content.Shared.Mobs.Systems;
using Content.Shared.SSDIndicator;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server._CP14.Construction.Condition;

[UsedImplicitly]
[DataDefinition]
public sealed partial class CP14SingletonNotExist : IGraphCondition
{
    [DataField(required: true)]
    public string Key = string.Empty;

    public bool Condition(EntityUid craft, IEntityManager entityManager)
    {
        if (!entityManager.TryGetComponent<TransformComponent>(craft, out var craftXform))
            return false;

        var query = entityManager.EntityQueryEnumerator<CP14SingletonComponent>();
        while (query.MoveNext(out var uid, out var singleton))
        {
            if (singleton.Key == Key)
                return false;
        }

        return true;
    }

    public bool DoExamine(ExaminedEvent args)
    {
        if (Condition(args.Examined, IoCManager.Resolve<IEntityManager>()))
            return false;

        args.PushMarkup(Loc.GetString("cp14-construction-condition-singleton"));
        return true;
    }

    public IEnumerable<ConstructionGuideEntry> GenerateGuideEntry()
    {
        yield return new ConstructionGuideEntry()
        {
            Localization = "cp14-construction-condition-singleton",
        };
    }
}
