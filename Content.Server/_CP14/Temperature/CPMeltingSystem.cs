using Content.Server.Fluids.EntitySystems;
using Content.Server.Temperature.Systems;

namespace Content.Server._CP14.Temperature;

public sealed partial class CPMeltingSystem : EntitySystem
{
    [Dependency] private readonly PuddleSystem _puddleSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CPMeltingComponent, OnTemperatureChangeEvent>(OnTemperatureChange);
    }

    private void OnTemperatureChange(Entity<CPMeltingComponent> ent, ref OnTemperatureChangeEvent args)
    {
        if (!(args.CurrentTemperature >= ent.Comp.MeltTemperature))
            return;

        if (ent.Comp.MeltSolution != null)
        {
            _puddleSystem.TrySplashSpillAt(ent, Transform(ent).Coordinates, ent.Comp.MeltSolution, out var puddleUid);
        }
        QueueDel(ent);
    }
}
