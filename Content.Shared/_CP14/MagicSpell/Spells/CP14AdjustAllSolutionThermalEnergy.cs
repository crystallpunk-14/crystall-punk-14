using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;

namespace Content.Shared._CP14.MagicSpell.Spells;

/// <summary>
///     Adjusts the thermal energy of all the solutions inside the container.
/// </summary>
public sealed partial class CP14AdjustAllSolutionThermalEnergy : CP14SpellEffect
{
    /// <summary>
    ///     The change in energy.
    /// </summary>
    [DataField(required: true)]
    public float Delta;

    /// <summary>
    ///     The minimum temperature this effect can reach.
    /// </summary>
    [DataField]
    public float? MinTemp;

    /// <summary>
    ///     The maximum temperature this effect can reach.
    /// </summary>
    [DataField]
    public float? MaxTemp;

    public override void Effect(EntityManager entManager, CP14SpellEffectBaseArgs args)
    {
        var solutionContainer = entManager.System<SharedSolutionContainerSystem>();

        if (!entManager.TryGetComponent<SolutionContainerManagerComponent>(args.Target, out var solutionComp))
            return;

        var target = new Entity<SolutionContainerManagerComponent?>(args.Target.Value, solutionComp);
        foreach (var (_, solution) in solutionContainer.EnumerateSolutions(target))
        {
            if (solution.Comp.Solution.Volume == 0)
                continue;

            var heatCap = solution.Comp.Solution.GetHeatCapacity(null);
            var deltaT = Delta / heatCap;
            var temperature = solution.Comp.Solution.Temperature + deltaT;

            if (deltaT > 0 && MaxTemp is { } maxTemp && temperature > maxTemp)
            {
                temperature = maxTemp;
                if (solution.Comp.Solution.Temperature > temperature)
                    continue;
            }
            else if (deltaT < 0 && MinTemp is { } minTemp && temperature < minTemp)
            {
                temperature = minTemp;
                if (solution.Comp.Solution.Temperature < temperature)
                    continue;
            }

            // Freezing below 0k is not possible.
            temperature = Math.Max(temperature, 0);
            solutionContainer.SetTemperature(solution, temperature);
        }
    }
}
