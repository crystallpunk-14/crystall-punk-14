using Content.Server.Atmos.Components;
using Content.Server.Temperature.Components;
using Content.Server.Temperature.Systems;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.FixedPoint;
using Content.Shared.Placeable;
using Robust.Server.GameObjects;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Server._CP14.Temperature;

public sealed partial class CP14TemperatureSystem : EntitySystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly TemperatureSystem _temperature = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;

    private readonly TimeSpan _updateTick = TimeSpan.FromSeconds(1f);
    private TimeSpan _timeToNextUpdate = TimeSpan.Zero;

    // The event was removed because it could have been triggered during an iteration

    private void DeferredTemperatureTransforms()
    {
        //Yes.. it's a crutch.
        //Unfortunately, the conversion system has strict requirements for requests.
        //And we technically can't request a conversion or deletion during integration, or everything will go to hell...
        var uids = new List<EntityUid>();
        var query = EntityQueryEnumerator<CP14TemperatureTransformationComponent, TemperatureComponent, TransformComponent>();

        while (query.MoveNext(out var uid, out _, out _, out _))
        {
            uids.Add(uid);
        }

        foreach (var start in uids)
        {
            if (!TryComp<CP14TemperatureTransformationComponent>(start, out var trans))
                continue;
            if (!TryComp<TemperatureComponent>(start, out var temp))
                continue;
            if (!TryComp<TransformComponent>(start, out var xform))
                continue;

            foreach (var entry in trans.Entries)
            {
                if (!(temp.CurrentTemperature > entry.TemperatureRange.X) ||
                    !(temp.CurrentTemperature < entry.TemperatureRange.Y) ||
                    entry.TransformTo == null)
                    continue;
                var result = Spawn(entry.TransformTo, xform.Coordinates);

                // DropNextTo has one problem, with a large number of objects, they may not be placed initially in the container, but spawned under it.
                // This happens because DropNextTo initially spawns outside the container and then attempts to place it in the container.
                if (_container.TryGetContainingContainer(start, out var container))
                {
                    _container.Remove(start, container);
                    _container.Insert(result, container);
                }
                else
                {
                    _transform.DropNextTo(result, (start, xform));
                }

                if (_solutionContainer.TryGetSolution(result, trans.Solution, out var resultSoln, out _) &&
                    _solutionContainer.TryGetSolution(start, trans.Solution, out var startSoln, out var startSolution))
                {
                    _solutionContainer.RemoveAllSolution(resultSoln.Value);
                    resultSoln.Value.Comp.Solution.MaxVolume = startSoln.Value.Comp.Solution.MaxVolume;
                    _solutionContainer.TryAddSolution(resultSoln.Value, startSolution);
                }

                QueueDel(start);
                break;
            }
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_timing.CurTime <= _timeToNextUpdate)
            return;

        _timeToNextUpdate = _timing.CurTime + _updateTick;

        FlammableEntityHeating();
        FlammableSolutionHeating();
        NormalizeSolutionTemperature();

        DeferredTemperatureTransforms();
    }

    private float GetTargetTemperature(FlammableComponent flammable, CP14FlammableSolutionHeaterComponent heater)
    {
        return flammable.FireStacks * heater.DegreesPerStack;
    }

    private void NormalizeSolutionTemperature()
    {
        var query = EntityQueryEnumerator<CP14SolutionTemperatureComponent, SolutionContainerManagerComponent>();
        while (query.MoveNext(out var uid, out var temp, out var container))
        {
            foreach (var (_, soln) in _solutionContainer.EnumerateSolutions((uid, container)))
            {
                if (TryAffectTemp(soln.Comp.Solution.Temperature,
                        temp.StandardTemp,
                        soln.Comp.Solution.Volume,
                        out var newT,
                        power: 0.05f))
                    _solutionContainer.SetTemperature(soln, newT);
            }
        }
    }
    private void FlammableEntityHeating()
    {
        var flammableQuery =
            EntityQueryEnumerator<CP14FlammableEntityHeaterComponent, ItemPlacerComponent, FlammableComponent>();
        while (flammableQuery.MoveNext(out _, out var heater, out var itemPlacer, out var flammable))
        {
            if (!flammable.OnFire)
                continue;

            var energy = flammable.FireStacks * heater.DegreesPerStack;

            foreach (var ent in itemPlacer.PlacedEntities)
            {
                _temperature.ChangeHeat(ent, energy);

                if (!TryComp<CP14TemperatureTransmissionComponent>(ent, out var transmission))
                    continue;

                if (!_container.TryGetContainer(ent, transmission.ContainerId, out var container))
                    continue;

                foreach (var containedEntity in container.ContainedEntities)
                {
                    _temperature.ChangeHeat(containedEntity, energy);

                }
            }
        }
    }

    private void FlammableSolutionHeating()
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

                foreach (var (_, soln) in _solutionContainer.EnumerateSolutions((heatingEntity, container)))
                {
                    if (TryAffectTemp(soln.Comp.Solution.Temperature,
                            GetTargetTemperature(flammable, heater),
                            soln.Comp.Solution.Volume,
                            out var newT))
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

        newT = (float)(oldT + (targetT - oldT) / mass * power);
        return true;
    }
}


