using JetBrains.Annotations;

namespace Content.Shared._CP14.MagicRitual;

[ImplicitDataDefinitionForInheritors]
[MeansImplicitUse]
public abstract partial class CP14RitualCondition
{
    public abstract void Check(IEntityManager entManager, EntityUid ritual);
}
