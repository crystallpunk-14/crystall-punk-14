using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicRitual;

[ImplicitDataDefinitionForInheritors]
[MeansImplicitUse]
public abstract partial class CP14RitualEffect
{
    public abstract void Effect(IEntityManager entMan, IPrototypeManager protoMan, EntityUid ritual);
}
