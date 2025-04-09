using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicRitual.Requirements;

/// <summary>
/// Requires that the stability of the ritual be within specified limits. If the stability is above or below the specified values, the check will fail
/// </summary>
public sealed partial class RequiredStability : CP14RitualRequirement
{
    [DataField]
    public float Min = 0;
    [DataField]
    public float Max = 1;

    public override string? GetGuidebookRequirementDescription(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        return Min switch
        {
            > 0 when Max < 1 =>
                   Loc.GetString("cp14-ritual-required-stability-minmax", ("min", Min*100), ("max", Max*100)),
            > 0 => Loc.GetString("cp14-ritual-required-stability-min", ("min", Min*100)),
            < 0 => Loc.GetString("cp14-ritual-required-stability-max", ("min", Max*100)),
            _ => null,
        };
    }

    public override bool Check(EntityManager entManager, Entity<CP14MagicRitualPhaseComponent> phaseEnt, float stability)
    {
        if (phaseEnt.Comp.Ritual is null)
            return false;

        if (!entManager.TryGetComponent<CP14MagicRitualComponent>(phaseEnt, out var ritualComp))
            return false;

        return !(ritualComp.Stability < Min) && !(ritualComp.Stability > Max);
    }
}
