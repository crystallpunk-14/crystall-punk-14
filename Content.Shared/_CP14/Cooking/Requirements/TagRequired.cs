using Content.Shared.Chemistry.Components;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Cooking.Requirements;

public sealed partial class TagRequired : CP14CookingCraftRequirement
{
    /// <summary>
    /// Any of this tags accepted
    /// </summary>
    [DataField(required: true)]
    public List<ProtoId<TagPrototype>> Tags = default!;

    [DataField]
    public byte Min = 1;

    public override bool CheckRequirement(IEntityManager entManager,
        IPrototypeManager protoManager,
        IReadOnlyList<EntityUid> placedEntities,
        List<ProtoId<TagPrototype>> placedTags,
        Solution? solution = null)
    {
        var count = 0;
        foreach (var placedTag in placedTags)
        {
            if (Tags.Contains(placedTag))
            {
                count++;
            }
        }

        return count >= Min;
    }

    public override float GetComplexity()
    {
        return Min;
    }
}
