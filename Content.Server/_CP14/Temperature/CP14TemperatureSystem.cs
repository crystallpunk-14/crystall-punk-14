using Content.Server.Atmos.Components;
using Content.Server.Temperature.Systems;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.FixedPoint;
using Content.Shared.Placeable;
using Content.Shared.Temperature;
using Robust.Server.GameObjects;
using Robust.Shared.Timing;

namespace Content.Server._CP14.Temperature;

public sealed partial class CP14TemperatureSystem : EntitySystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly TemperatureSystem _temperature = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    private readonly TimeSpan _updateTick = TimeSpan.FromSeconds(1f);
    private TimeSpan _timeToNextUpdate = TimeSpan.Zero;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14TemperatureTransformationComponent, OnTemperatureChangeEvent>(OnTemperatureChanged);
    }

    private void OnTemperatureChanged(Entity<CP14TemperatureTransformationComponent> start,
        ref OnTemperatureChangeEvent args)
    {
        var xform = Transform(start);
        foreach (var entry in start.Comp.Entries)
        {
            if (args.CurrentTemperature > entry.TemperatureRange.X &&
                args.CurrentTemperature < entry.TemperatureRange.Y)
            {
                if (entry.TransformTo is not null)
                {
                    var result = SpawnAtPosition(entry.TransformTo, xform.Coordinates);

                    //Try putting in container
                    _transform.DropNextTo(result, (start, xform));

                    if (_solutionContainer.TryGetSolution(result,
                            start.Comp.Solution,
                            out var resultSoln,
                            out _)
                        && _solutionContainer.TryGetSolution(start.Owner,
                            start.Comp.Solution,
                            out var startSoln,
                            out var startSolution))
                    {
                        _solutionContainer.RemoveAllSolution(resultSoln.Value); //Remove all YML reagents
                        resultSoln.Value.Comp.Solution.MaxVolume = startSoln.Value.Comp.Solution.MaxVolume;
                        _solutionContainer.TryAddSolution(resultSoln.Value, startSolution);
                    }
                }

                Del(start);
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
