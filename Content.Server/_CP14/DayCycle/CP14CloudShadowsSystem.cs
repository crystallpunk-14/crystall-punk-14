using System.Numerics;
using Content.Shared._CP14.DayCycle;
using Robust.Shared.Random;

namespace Content.Server._CP14.DayCycle;

public sealed partial class CP14CloudShadowsSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14CloudShadowsComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<CP14CloudShadowsComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.CloudSpeed = new Vector2(
            _random.NextFloat(-ent.Comp.MaxSpeed, ent.Comp.MaxSpeed),
            _random.NextFloat(-ent.Comp.MaxSpeed, ent.Comp.MaxSpeed));
    }
}
