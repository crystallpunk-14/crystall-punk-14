using Content.Server.Atmos.Components;
using Content.Server.Chemistry.Containers.EntitySystems;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.FixedPoint;
using Content.Shared.Placeable;
using Robust.Shared.Timing;

namespace Content.Server._CP14.Temperature;

public sealed partial class CP14SolutionTemperatureSystem : EntitySystem
{
    [Dependency] private readonly SolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private TimeSpan _updateTick = TimeSpan.FromSeconds(1f);
    private TimeSpan _timeToNextUpdate = TimeSpan.Zero;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_timing.CurTime <= _timeToNextUpdate)
            return;

        _timeToNextUpdate = _timing.CurTime + _updateTick;

        FlammableHeating();
        NormalizeTemperature();
    }

    private void NormalizeTemperature()
    {
        var query = EntityQueryEnumerator<CP14SolutionTemperatureComponent, SolutionContainerManagerComponent>();
        while (query.MoveNext(out var uid, out var temp, out var container))
        {
            foreach (var (_, soln) in _solutionContainer.EnumerateSolutions((uid, container)))
            {
                if (TryAffectTemp(soln.Comp.Solution.Temperature, temp.StandardTemp, soln.Comp.Solution.Volume, out var newT, power: 0.05f))
                    _solutionContainer.SetTemperature(soln, newT);
            }
        }
    }
    private void FlammableHeating()
    {
        var query =
            EntityQueryEnumerator<CP14FlammableSolutionHeaterComponent, ItemPlacerComponent, FlammableComponent>();
        while (query.MoveNext(out _, out var heater, out var itemPlacer, out var flammable))
        {
            foreach (var heatingEntity in itemPlacer.PlacedEntities)
            {
                if (!flammable.OnFire)
                    continue;

                if (!TryComp<SolutionContainerManagerComponent>(heatingEntity, out var container))
                    continue;

                var targetT = flammable.FireStacks * heater.DegreesPerStack;
                foreach (var (_, soln) in _solutionContainer.EnumerateSolutions((heatingEntity, container)))
                {
                    if (TryAffectTemp(soln.Comp.Solution.Temperature, targetT, soln.Comp.Solution.Volume, out var newT))
                        _solutionContainer.SetTemperature(soln, newT);
                }
            }
        }
    }

    private static bool TryAffectTemp(float oldT, float targetT, FixedPoint2 mass, out float newT, float power = 1)
    {
        newT = oldT;

        if (mass == 0)
            return false;

        newT = (float) (oldT + ((targetT - oldT) / mass) * power);
        return true;
    }
}
