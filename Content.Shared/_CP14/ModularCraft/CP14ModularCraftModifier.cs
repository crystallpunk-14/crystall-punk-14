using JetBrains.Annotations;

namespace Content.Shared._CP14.ModularCraft;

[ImplicitDataDefinitionForInheritors]
[MeansImplicitUse]
public abstract partial class CP14ModularCraftModifier
{
    /// <summary>
    /// An effect caused by the first stage of modifications to an item. Use it to add new components,
    /// or make significant changes to the basic entity.
    /// </summary>
    public virtual void Effect(EntityManager entManager, EntityUid uid)
    {

    }

    /// <summary>
    /// An effect that is invoked in the second step of object assembly. Place effects that modify components here.
    /// </summary>
    public virtual void PostEffect(EntityManager entManager, EntityUid uid)
    {

    }
}
