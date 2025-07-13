using Content.Shared.Chemistry.Components;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Cooking;

[ImplicitDataDefinitionForInheritors]
[MeansImplicitUse]
public abstract partial class CP14CookingCraftRequirement
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="entManager"></param>
    /// <param name="protoManager"></param>
    /// <param name="placedEntities"></param>
    /// <returns></returns>
    public abstract bool CheckRequirement(IEntityManager entManager,
        IPrototypeManager protoManager,
        IReadOnlyList<EntityUid> placedEntities,
        Solution? solution = null);

    public abstract float GetComplexity();
}
