using Content.Server.Xenoarchaeology.XenoArtifacts;
using Content.Shared.Chemistry.Reagent;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Chemistry.ReagentEffect;

[UsedImplicitly]
[DataDefinition]
public sealed partial class CP14AffectSolutionTemperature : Shared.Chemistry.Reagent.ReagentEffect
{
    /// <summary>
    /// Temperature added to the solution. If negative, the solution is cooling.
    /// </summary>
    [DataField]
    private float AddTemperature = -300f;

    public override void Effect(ReagentEffectArgs args)
    {
        if (args.Source != null)
            args.Source.Temperature += AddTemperature;
    }

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        return Loc.GetString("cp14-reagent-effect-guidebook-temp-affect",
            ("chance", Probability),
            ("temperature", AddTemperature));
    }
}
