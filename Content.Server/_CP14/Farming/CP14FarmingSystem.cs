using Content.Shared._CP14.Farming;
using Content.Shared._CP14.Farming.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Damage;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._CP14.Farming;

public sealed partial class CP14FarmingSystem : CP14SharedFarmingSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;

    public override void Initialize()
    {
        base.Initialize();

        InitializeResources();

        SubscribeLocalEvent<CP14PlantComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<CP14PlantAutoRootComponent, MapInitEvent>(OnAutoRootMapInit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CP14PlantComponent>();
        while (query.MoveNext(out var uid, out var plant))
        {
            if (_timing.CurTime <= plant.NextUpdateTime)
                continue;

            var newTime = _random.NextFloat(plant.UpdateFrequency);
            plant.NextUpdateTime = _timing.CurTime + TimeSpan.FromSeconds(newTime);

            var ev = new CP14PlantUpdateEvent((uid, plant));
            RaiseLocalEvent(uid, ev);

            AffectResource((uid, plant), ev.ResourceDelta);
            AffectEnergy((uid, plant), ev.EnergyDelta);

            var ev2 = new CP14AfterPlantUpdateEvent((uid, plant));
            RaiseLocalEvent(uid, ev2);

            Dirty(uid, plant);
        }
    }

    private void OnMapInit(Entity<CP14PlantComponent> plant, ref MapInitEvent args)
    {
        var newTime = _random.NextFloat(plant.Comp.UpdateFrequency);
        plant.Comp.NextUpdateTime = _timing.CurTime + TimeSpan.FromSeconds(newTime);
    }

    private void OnAutoRootMapInit(Entity<CP14PlantAutoRootComponent> autoRoot, ref MapInitEvent args)
    {
        var grid = Transform(autoRoot).GridUid;
        if (grid == null || !TryComp<MapGridComponent>(grid, out var gridComp))
            return;

        var targetPos = new EntityCoordinates(grid.Value,Transform(autoRoot).LocalPosition);
        var anchored = _map.GetAnchoredEntities(grid.Value, gridComp, targetPos);

        foreach (var entt in anchored)
        {
            if (!TryComp<CP14SoilComponent>(entt, out var soil))
                continue;

            soil.PlantUid = autoRoot;

            if (TryComp<CP14PlantComponent>(autoRoot, out var plantComp))
            {
                plantComp.SoilUid = entt;
            }

            break;
        }
    }
}
