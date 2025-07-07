/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using Content.Shared.Tag;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.Workbench.Requirements;

public sealed partial class TagResource : CP14WorkbenchCraftRequirement
{
    [DataField(required: true)]
    public ProtoId<TagPrototype> Tag;

    [DataField]
    public int Count = 1;

    [DataField(required: true)]
    public LocId? Title;

    [DataField(required: true)]
    public SpriteSpecifier? Texture;

    public override bool CheckRequirement(
        IEntityManager entManager,
        IPrototypeManager protoManager,
        HashSet<EntityUid> placedEntities)
    {
        var tagSystem = entManager.System<TagSystem>();

        var count = 0;
        foreach (var ent in placedEntities)
        {
            if (!tagSystem.HasTag(ent, Tag))
                continue;

            count++;
        }

        if (count < Count)
            return false;

        return true;
    }

    public override void PostCraft(IEntityManager entManager, IPrototypeManager protoManager, HashSet<EntityUid> placedEntities)
    {
        var tagSystem = entManager.System<TagSystem>();

        var requiredCount = Count;
        foreach (var placedEntity in placedEntities)
        {
            if (!tagSystem.HasTag(placedEntity, Tag))
                continue;

            if (requiredCount <= 0)
                break;

            requiredCount--;
            entManager.DeleteEntity(placedEntity);
        }
    }

    public override string GetRequirementTitle(IPrototypeManager protoManager)
    {
        if (Title is null)
            return "Error tag name";

        return $"{Loc.GetString(Title)} x{Count}";
    }

    public override SpriteSpecifier? GetRequirementTexture(IPrototypeManager protoManager)
    {
        return Texture;
    }
}
