using JetBrains.Annotations;

namespace Content.Server._CP14.MagicRituals.Components.Actions;

[ImplicitDataDefinitionForInheritors]
[MeansImplicitUse]
public abstract partial class CP14RitualAction
{
    public abstract void Effect(EntityManager entManager, EntityUid phaseEnt);
}
