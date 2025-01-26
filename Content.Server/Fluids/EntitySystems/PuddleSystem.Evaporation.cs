using Content.Server.Weather;
using Content.Shared.Chemistry.Components;
using Content.Shared.FixedPoint;
using Content.Shared.Fluids.Components;
using Content.Shared.Maps;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Server.Fluids.EntitySystems;

public sealed partial class PuddleSystem
{
    [Dependency] private readonly SharedMapSystem _maps = default!; //CP14
    [Dependency] private readonly WeatherSystem _weather = default!; //CP14
    private static readonly TimeSpan EvaporationCooldown = TimeSpan.FromSeconds(1);

    private void OnEvaporationMapInit(Entity<EvaporationComponent> entity, ref MapInitEvent args)
    {
        entity.Comp.NextTick = _timing.CurTime + EvaporationCooldown;


        //CP14 Force evaporation under sky
        var xform = Transform(entity);
        if (TryComp<MapGridComponent>(xform.GridUid, out var mapGrid))
        {
            var tileRef = _maps.GetTileRef(xform.GridUid.Value, mapGrid, xform.Coordinates);
            entity.Comp.CP14ForceEvaporation = _weather.CanWeatherAffect(xform.GridUid.Value, mapGrid, tileRef);
        }
        //CP14 End force evaporation under sky
    }

    private void UpdateEvaporation(EntityUid uid, Solution solution)
    {
        if (HasComp<EvaporationComponent>(uid))
        {
            return;
        }

        if (solution.GetTotalPrototypeQuantity(EvaporationReagents) > FixedPoint2.Zero)
        {
            var evaporation = AddComp<EvaporationComponent>(uid);
            evaporation.NextTick = _timing.CurTime + EvaporationCooldown;
            return;
        }

        RemComp<EvaporationComponent>(uid);
    }

    private void TickEvaporation()
    {
        var query = EntityQueryEnumerator<EvaporationComponent, PuddleComponent>();
        var xformQuery = GetEntityQuery<TransformComponent>();
        var curTime = _timing.CurTime;
        while (query.MoveNext(out var uid, out var evaporation, out var puddle))
        {
            if (evaporation.NextTick > curTime)
                continue;

            evaporation.NextTick += EvaporationCooldown;

            if (!_solutionContainerSystem.ResolveSolution(uid, puddle.SolutionName, ref puddle.Solution, out var puddleSolution))
                continue;

            var reagentTick = evaporation.EvaporationAmount * EvaporationCooldown.TotalSeconds;

            //CP14 Force evaporation
            if (!evaporation.CP14ForceEvaporation)
                puddleSolution.SplitSolutionWithOnly(reagentTick, EvaporationReagents);
            else
                puddleSolution.SplitSolution(reagentTick);
            //CP14 end force evaporation

            // Despawn if we're done
            if (puddleSolution.Volume == FixedPoint2.Zero)
            {
                // Spawn a *sparkle*
                Spawn("PuddleSparkle", xformQuery.GetComponent(uid).Coordinates);
                QueueDel(uid);
            }

            _solutionContainerSystem.UpdateChemicals(puddle.Solution.Value);
        }
    }
}
