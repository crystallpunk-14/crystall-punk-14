using Content.Shared.Chemistry.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Cooking.Requirements;

public sealed partial class AlwaysMet : CP14CookingCraftRequirement
{
    public override bool CheckRequirement(IEntityManager entManager,
        IPrototypeManager protoManager,
        IReadOnlyList<EntityUid> placedEntities,
        Solution? solution = null)
    {
        return true;
    }

    public override float GetComplexity()
    {
        return -100;
    }
}
