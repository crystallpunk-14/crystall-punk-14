using Content.Shared.EntityEffects;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Chemistry.ReagentEffect;

[UsedImplicitly]
[DataDefinition]
public sealed partial class CP14AffectSolutionTemperature : EntityEffect
{
    /// <summary>
    /// Temperature added to the solution. If negative, the solution is cooling.
    /// </summary>
    [DataField]
    public float AddTemperature = -300f;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        return Loc.GetString("cp14-reagent-effect-guidebook-temp-affect",
            ("chance", Probability),
            ("temperature", AddTemperature));
    }

    public override void Effect(EntityEffectBaseArgs args)
    {
        if (args is EntityEffectReagentArgs reagentArgs)
        {
            if (reagentArgs.Source != null)
                reagentArgs.Source.Temperature += AddTemperature;

            return;
        }

        // TODO: Someone needs to figure out how to do this for non-reagent effects.
        throw new NotImplementedException();
    }
}
