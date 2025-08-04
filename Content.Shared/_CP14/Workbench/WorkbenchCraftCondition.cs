using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.Workbench;

[ImplicitDataDefinitionForInheritors]
[MeansImplicitUse]
public abstract partial class CP14WorkbenchCraftCondition
{
    public abstract bool CheckCondition(
        EntityManager entManager,
        IPrototypeManager protoManager,
        EntityUid workbench,
        EntityUid user);

    public virtual void PostCraft(
        EntityManager entManager,
        IPrototypeManager protoManager,
        EntityUid workbench,
        EntityUid user)
    {

    }

    public abstract void FailedEffect(
        EntityManager entManager,
        IPrototypeManager protoManager,
        EntityUid workbench,
        EntityUid user);

    /// <summary>
    /// This text will be displayed in the description of the craft conditions. Write something like ‘The workbench must be filled to 100% mana.’ here
    /// </summary>
    public virtual string GetConditionTitle(
        EntityManager entManager,
        IPrototypeManager protoManager,
        EntityUid workbench,
        EntityUid user)
    {
        return string.Empty;
    }

    /// <summary>
    /// You can specify the texture directly. Return null to disable.
    /// </summary>
    public virtual SpriteSpecifier? GetConditionTexture(IPrototypeManager protoManager)
    {
        return null;
    }
}
