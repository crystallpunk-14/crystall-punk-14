namespace Content.Server._CP14.MagicRituals.Components.Requirements;

/// <summary>
/// Requires that the stability of the ritual be within specified limits. If the stability is above or below the specified values, the check will fail
/// </summary>
public sealed partial class RequiredStability : CP14RitualRequirement
{
    [DataField]
    public float Min = 0;
    [DataField]
    public float Max = 1;

    public override bool Check(EntityManager entManager, Entity<CP14MagicRitualPhaseComponent> phaseEnt, float stability)
    {
        if (phaseEnt.Comp.Ritual is null)
            return false;

        var s = phaseEnt.Comp.Ritual.Value.Comp.Stability;

        return !(s < Min) && !(s > Max);
    }
}
