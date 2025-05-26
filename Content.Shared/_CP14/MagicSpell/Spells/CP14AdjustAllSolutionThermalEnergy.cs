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

            var maxTemp = MaxTemp ?? float.PositiveInfinity;
            var minTemp = Math.Max(MinTemp ?? 0, 0);
            var oldTemp = solution.Comp.Solution.Temperature;

            if (Delta > 0 && oldTemp >= maxTemp)
                continue;
            if (Delta < 0 && oldTemp <= minTemp)
                continue;

            var heatCap = solution.Comp.Solution.GetHeatCapacity(null);
            var deltaT = Delta / heatCap;

            solutionContainer.SetTemperature(solution, Math.Clamp(oldTemp + deltaT, minTemp, maxTemp));
        }
    }
}
