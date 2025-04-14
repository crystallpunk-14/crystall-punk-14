/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using Content.Shared._CP14.Workbench.Prototypes;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.Workbench.Requirements;

public sealed partial class TagResource : CP14WorkbenchCraftRequirement
{
    public override bool HideRecipe { get; set; } = false;

    [DataField(required: true)]
    public ProtoId<TagPrototype> Tag;

    [DataField]
    public int Count = 1;

    [DataField(required: true)]
    public LocId? Title;

    [DataField(required: true)]
    public SpriteSpecifier? Texture;

    public override bool CheckRequirement(
        EntityManager entManager,
        IPrototypeManager protoManager,
        HashSet<EntityUid> placedEntities,
        EntityUid user,
        CP14WorkbenchRecipePrototype recipe)
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

    public override void PostCraft(EntityManager entManager, IPrototypeManager protoManager, HashSet<EntityUid> placedEntities, EntityUid user)
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

    public override EntityPrototype? GetRequirementEntityView(IPrototypeManager protoManager)
    {
        return null;
    }

    public override SpriteSpecifier? GetRequirementTexture(IPrototypeManager protoManager)
    {
        return Texture;
    }
}
