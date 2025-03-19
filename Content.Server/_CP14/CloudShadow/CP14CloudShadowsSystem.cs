using Content.Shared._CP14.CloudShadow;
using Robust.Shared.Random;

namespace Content.Server._CP14.CloudShadow;

public sealed class CP14CloudShadowsSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14CloudShadowsComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<CP14CloudShadowsComponent> entity, ref MapInitEvent args)
    {
        entity.Comp.CloudSpeed = _random.NextVector2(-entity.Comp.MaxSpeed, entity.Comp.MaxSpeed);
    }
}
