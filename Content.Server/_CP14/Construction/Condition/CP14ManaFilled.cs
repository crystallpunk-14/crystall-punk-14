using Content.Shared._CP14.MagicEnergy.Components;
using Content.Shared.Construction;
using Content.Shared.Examine;
using JetBrains.Annotations;
using Robust.Shared.Utility;

namespace Content.Server._CP14.Construction.Condition;

/// <summary>
///     Makes the condition fail if any entities on a tile have (or not) a component.
/// </summary>
[UsedImplicitly]
[DataDefinition]
public sealed partial class CP14ManaFilled : IGraphCondition
{
    public bool Condition(EntityUid uid, IEntityManager entityManager)
    {
        if (!entityManager.TryGetComponent(uid, out CP14MagicEnergyContainerComponent? container))
            return true;

        return container.Energy >= container.MaxEnergy;
    }

    public bool DoExamine(ExaminedEvent args)
    {
        if (Condition(args.Examined, IoCManager.Resolve<IEntityManager>()))
            return false;

        args.PushMarkup(Loc.GetString("cp14-construction-condition-mana-filled"));
        return true;
    }

    public IEnumerable<ConstructionGuideEntry> GenerateGuideEntry()
    {
        yield return new ConstructionGuideEntry()
        {
            Localization = "cp14-construction-condition-mana-filled",
            Icon = new SpriteSpecifier.Rsi(new ResPath("/Textures/_CP14/Actions/Spells/meta.rsi"), "mana"),
        };
    }
}
