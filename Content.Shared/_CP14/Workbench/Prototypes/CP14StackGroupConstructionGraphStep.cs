/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using System.Diagnostics.CodeAnalysis;
using Content.Shared.Construction;
using Content.Shared.Construction.Steps;
using Content.Shared.Examine;
using Content.Shared.Stacks;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.Workbench.Prototypes;

[DataDefinition]
public sealed partial class CP14StackGroupConstructionGraphStep : EntityInsertConstructionGraphStep
{
    [DataField]
    public ProtoId<CP14StackGroupPrototype> StackGroup = default!;

    [DataField]
    public int Amount = 1;

    public override void DoExamine(ExaminedEvent examinedEvent)
    {
        var group = IoCManager.Resolve<IPrototypeManager>().Index(StackGroup);

        examinedEvent.PushMarkup(Loc.GetString("construction-insert-material-entity", ("amount", Amount), ("materialName", Loc.GetString(group.Name))));
    }

    public override bool EntityValid(EntityUid uid, IEntityManager entityManager, IComponentFactory compFactory)
    {
        var group = IoCManager.Resolve<IPrototypeManager>().Index(StackGroup);

        return entityManager.TryGetComponent(uid, out StackComponent? stack) && group.Stacks.Contains(stack.StackTypeId) && stack.Count >= Amount;
    }

    public bool EntityValid(EntityUid entity, [NotNullWhen(true)] out StackComponent? stack)
    {
        var group = IoCManager.Resolve<IPrototypeManager>().Index(StackGroup);

        if (IoCManager.Resolve<IEntityManager>().TryGetComponent(entity, out StackComponent? otherStack) && group.Stacks.Contains(otherStack.StackTypeId) && otherStack.Count >= Amount)
            stack = otherStack;
        else
            stack = null;

        return stack != null;
    }

    public override ConstructionGuideEntry GenerateGuideEntry()
    {
        var proto = IoCManager.Resolve<IPrototypeManager>();
        var group = proto.Index(StackGroup);

        var firstStack = group.Stacks.FirstOrNull();

        return new ConstructionGuideEntry()
        {
            Localization = "construction-presenter-material-step",
            Arguments = new (string, object)[]{("amount", Amount), ("material", Loc.GetString(group.Name))},
            Icon = firstStack != null ? proto.Index(firstStack.Value).Icon : SpriteSpecifier.Invalid,
        };
    }
}
