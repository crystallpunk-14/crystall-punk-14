using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.EntityEffects;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Server.EntityEffects.Effects;

/// <summary>
///     Adjusts the thermal energy of all the solutions inside the container.
/// </summary>
[UsedImplicitly]
[DataDefinition]
public sealed partial class AdjustAllSolutionThermalEnergy : EntityEffect
{
    /// <summary>
    ///     The change in energy.
    /// </summary>
    [DataField("delta", required: true)] private float _delta;

    /// <summary>
    ///     The minimum temperature this effect can reach.
    /// </summary>
    [DataField("minTemp")] private float _minTemp = 0.0f;

    /// <summary>
    ///     The maximum temperature this effect can reach.
    /// </summary>
    [DataField("maxTemp")] private float _maxTemp = float.PositiveInfinity;

    public override void Effect(EntityEffectBaseArgs args)
    {
        var solutionContainer = args.EntityManager.System<SharedSolutionContainerSystem>();
        foreach (var (_, solution) in solutionContainer.EnumerateSolutions(args.TargetEntity))
        {
            if (solution.Comp.Solution.Volume == 0)
                continue;

            if (_delta > 0 && solution.Comp.Solution.Temperature >= _maxTemp)
                return;
            if (_delta < 0 && solution.Comp.Solution.Temperature <= _minTemp)
                return;

            var heatCap = solution.Comp.Solution.GetHeatCapacity(null);
            var deltaT = _delta / heatCap;

            solution.Comp.Solution.Temperature = Math.Clamp(solution.Comp.Solution.Temperature + deltaT, _minTemp, _maxTemp);
        }
    }

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    => Loc.GetString("reagent-effect-guidebook-adjust-solution-temperature-effect",
        ("chance", Probability), ("deltasign", MathF.Sign(_delta)), ("mintemp", _minTemp), ("maxtemp", _maxTemp));
}
