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

        SubscribeLocalEvent<CP14SeedbedComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<CP14SeedbedComponent, PlantSeedDoAfterEvent>(OnSeedPlantedDoAfter);
    }
    private void OnInteractUsing(Entity<CP14SeedbedComponent> seedbed, ref InteractUsingEvent args)
    {
        if (!TryComp<CP14SeedComponent>(args.Used, out var seed))
            return;

        //Audio
        //Popup

        var doAfterArgs =
            new DoAfterArgs(EntityManager, args.User, seed.PlantingTime, new PlantSeedDoAfterEvent(), seedbed, args.Used, seedbed)
            {
                BreakOnDamage = true,
                BlockDuplicate = true,
                BreakOnMove = true,
                BreakOnHandChange = true
            };
        _doAfterSystem.TryStartDoAfter(doAfterArgs);
    }

    private void OnSeedPlantedDoAfter(Entity<CP14SeedbedComponent> ent, ref PlantSeedDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Args.Used == null)
            return;

        QueueDel(args.Target); //delete seed
    }
}
