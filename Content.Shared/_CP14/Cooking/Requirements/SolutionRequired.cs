using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Cooking.Requirements;

public sealed partial class SolutionRequired : CP14CookingCraftRequirement
{
    /// <summary>
    /// Any of this tags accepted
    /// </summary>
    [DataField(required: true)]
    public ProtoId<ReagentPrototype> Reagent = default!;

    [DataField]
    public FixedPoint2 Min = 1;

    public override bool CheckRequirement(IEntityManager entManager,
        IPrototypeManager protoManager,
        IReadOnlyList<EntityUid> placedEntities,
        List<ProtoId<TagPrototype>> placedTags,
        Solution? solution = null)
    {
        if (solution is null)
            return false;

        foreach (var reagent in solution.Contents)
        {
            if (reagent.Reagent.Prototype != Reagent)
                continue;

            if (reagent.Quantity < Min)
                continue;

            return true;
        }

        return false;
    }

    public override float GetComplexity()
    {
        return (float)Min;
    }
}
