using Content.Shared._CP14.Farming;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;

namespace Content.Server._CP14.Farming;

public sealed partial class CP14FarmingSystem : CP14SharedFarmingSystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14SoilComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<CP14SoilComponent, PlantSeedDoAfterEvent>(OnSeedPlantedDoAfter);

        SubscribeLocalEvent<CP14PlantComponent, MapInitEvent>(OnMapInit);
    }

    private void OnInteractUsing(Entity<CP14SoilComponent> soil, ref InteractUsingEvent args)
    {
        if (!TryComp<CP14SeedComponent>(args.Used, out var seed))
            return;

        //Audio
        //Popup

        var doAfterArgs =
            new DoAfterArgs(EntityManager, args.User, seed.PlantingTime, new PlantSeedDoAfterEvent(), soil, args.Used, soil)
            {
                BreakOnDamage = true,
                BlockDuplicate = true,
                BreakOnMove = true,
                BreakOnHandChange = true
            };
        _doAfterSystem.TryStartDoAfter(doAfterArgs);
    }

    private void OnSeedPlantedDoAfter(Entity<CP14SoilComponent> ent, ref PlantSeedDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Args.Used == null)
            return;

        if (!TryComp<CP14SeedComponent>(args.Target, out var seed))
            return;

        var plant = SpawnAttachedTo(seed.PlantProto, Transform(ent).Coordinates);

        if (TryComp<CP14PlantComponent>(plant, out var plantComp))
        {
            TryRootPlant(plantComp, ent);
        }
        //Audio
        QueueDel(args.Target); //delete seed
    }

    private bool TryRootPlant(CP14PlantComponent component, Entity<CP14SoilComponent> soilUid)
    {
        //Parenting to soil here?

        component.SoilUid = soilUid;
        return true;
    }

    private void OnMapInit(Entity<CP14PlantComponent> plant, ref MapInitEvent args)
    {

    }

}
