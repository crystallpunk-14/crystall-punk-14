/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using Content.Shared.Stacks;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.Workbench.Requirements;

public sealed partial class MaterialResource : CP14WorkbenchCraftRequirement
{
    public override bool CheckRequirement(EntityManager entManager, IPrototypeManager protoManager, HashSet<EntityUid> placedEntities, EntityUid user)
    {
        throw new NotImplementedException();
    }

    public override void PostCraft(EntityManager entManager, HashSet<EntityUid> placedEntities, EntityUid user)
    {
        throw new NotImplementedException();
    }

    public override string GetRequirementTitle(IPrototypeManager protoManager)
    {
        throw new NotImplementedException();
    }

    public override EntityPrototype? GetRequirementEntityView(IPrototypeManager protoManager)
    {
        throw new NotImplementedException();
    }

    public override SpriteSpecifier? GetRequirementTexture(IPrototypeManager protoManager)
    {
        throw new NotImplementedException();
    }
}
