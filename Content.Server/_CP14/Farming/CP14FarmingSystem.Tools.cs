using Content.Server._CP14.Farming.Components;
using Content.Shared._CP14.Farming;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;

namespace Content.Server._CP14.Farming;

public sealed partial class CP14FarmingSystem
{
    private void InitializeTools()
    {
        SubscribeLocalEvent<CP14SeedComponent, AfterInteractEvent>(OnSeedInteract);

        SubscribeLocalEvent<CP14SoilComponent, PlantSeedDoAfterEvent>(OnSeedPlantedDoAfter);
    }

    private void OnSeedInteract(Entity<CP14SeedComponent> seed, ref AfterInteractEvent args)
    {
        if (!TryComp<CP14SoilComponent>(args.Target, out var soil))
            return;

        if (EntityManager.EntityExists(soil.PlantUid))
        {
            _popup.PopupEntity(Loc.GetString("cp14-farming-soil-interact-plant-exist"), args.Target.Value, args.User);
            return;
        }
        var doAfterArgs =
            new DoAfterArgs(EntityManager, args.User, seed.Comp.PlantingTime, new PlantSeedDoAfterEvent(), args.Target, args.Used, args.Target)
            {
                BreakOnDamage = true,
                BlockDuplicate = true,
                BreakOnMove = true,
                BreakOnHandChange = true,
            };
        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    public bool TryPlantSeed(EntityUid seed, EntityUid soil, EntityUid? user)
    {
        if (!TryComp<CP14SoilComponent>(soil, out var soilComp))
            return false;

        if (!TryComp<CP14SeedComponent>(seed, out var seedComp))
            return false;

        if (Exists(soilComp.PlantUid))
        {
            if (user is not null)
                _popup.PopupEntity(Loc.GetString("cp14-farming-soil-interact-plant-exist"), soil, user.Value);

            return false;
        }

        var plant = SpawnAttachedTo(seedComp.PlantProto, Transform(soil).Coordinates);

        if (!TryComp<CP14PlantComponent>(plant, out var plantComp))
            return false;

        _transform.SetParent(plant, soil);
        soilComp.PlantUid = plant;
        plantComp.SoilUid = soil;

        return true;
    }

    private void OnSeedPlantedDoAfter(Entity<CP14SoilComponent> soil, ref PlantSeedDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Args.Used == null || args.Target == null)
            return;

        if (!TryPlantSeed(args.Target.Value, soil, args.User))
            return;

        //Audio
        QueueDel(args.Target); //delete seed

        args.Handled = true;
    }
}
