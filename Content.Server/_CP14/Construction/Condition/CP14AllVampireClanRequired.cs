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
public sealed partial class CP14AllVampireClanRequired : IGraphCondition
{
    [DataField(required: true)]
    public ProtoId<CP14VampireFactionPrototype> Faction;

    public bool Condition(EntityUid craft, IEntityManager entityManager)
    {
        if (!entityManager.TryGetComponent<TransformComponent>(craft, out var craftXform))
            return false;

        var mobState = entityManager.System<MobStateSystem>();

        var query = entityManager.EntityQueryEnumerator<CP14VampireComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var vampire, out var xform))
        {
            if (vampire.Faction != Faction)
                continue;

            if (mobState.IsDead(uid))
                continue;

            if (entityManager.TryGetComponent<SSDIndicatorComponent>(uid, out var ssd) && ssd.IsSSD)
                continue;

            //Check distance to the vampire
            if (!xform.Coordinates.TryDistance(entityManager, craftXform.Coordinates, out var distance) || distance > 2)
                return false;
        }

        return true;
    }

    public bool DoExamine(ExaminedEvent args)
    {
        if (Condition(args.Examined, IoCManager.Resolve<IEntityManager>()))
            return false;

        args.PushMarkup(Loc.GetString("cp14-magic-spell-need-all-vampires"));
        return true;
    }

    public IEnumerable<ConstructionGuideEntry> GenerateGuideEntry()
    {
        yield return new ConstructionGuideEntry()
        {
            Localization = "cp14-magic-spell-need-all-vampires",
            Icon = new SpriteSpecifier.Rsi(new ResPath("/Textures/_CP14/Actions/Spells/vampire.rsi"), "blood_moon"),
        };
    }
}
