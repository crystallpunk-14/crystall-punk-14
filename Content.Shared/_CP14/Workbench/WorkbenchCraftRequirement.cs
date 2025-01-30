/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using JetBrains.Annotations;
using Robust.Client.Graphics;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.Workbench;

[ImplicitDataDefinitionForInheritors]
[MeansImplicitUse]
public abstract partial class CP14WorkbenchCraftRequirement
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="entManager"></param>
    /// <param name="protoManager"></param>
    /// <param name="placedEntities"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    public abstract bool CheckRequirement(EntityManager entManager,
        IPrototypeManager protoManager,
        HashSet<EntityUid> placedEntities,
        EntityUid user);

    /// <summary>
    ///
    /// </summary>
    /// <param name="entManager"></param>
    /// <param name="placedEntities"></param>
    /// <param name="resultEntities"></param>
    /// <param name="user"></param>
    public abstract void PostCraft(EntityManager entManager,
        HashSet<EntityUid> placedEntities,
        HashSet<EntityUid> resultEntities,
        EntityUid user);

    //For UI representation
    public abstract string GetRequirementTitle(IPrototypeManager protoManager);
    public abstract EntityPrototype? GetRequirementEntityView(IPrototypeManager protoManager);
    public abstract SpriteSpecifier? GetRequirementTexture(IPrototypeManager protoManager);
}
