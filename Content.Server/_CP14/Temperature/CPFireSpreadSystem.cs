using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.CrystallPunk.Temperature;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._CP14.Temperature;

public sealed partial class CPFireSpreadSystem : EntitySystem
{

    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IRobustRandom _random = default!;


    public override void Initialize()
    {
        SubscribeLocalEvent <CPFireSpreadComponent, OnFireChangedEvent>(OnCompInit);
    }

    private void OnCompInit(Entity<CPFireSpreadComponent> ent, ref OnFireChangedEvent args)
    {
        if (!args.OnFire)
            return;

        var cooldown = _random.NextFloat(ent.Comp.SpreadCooldownMin, ent.Comp.SpreadCooldownMax);
        ent.Comp.NextSpreadTime = _gameTiming.CurTime + TimeSpan.FromSeconds(cooldown);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CPFireSpreadComponent, FlammableComponent>();
        while (query.MoveNext(out var uid, out var spread, out var flammable))
        {
            if (!flammable.OnFire)
                continue;

            if (spread.NextSpreadTime < _gameTiming.CurTime) // Spread
            {
                var targets = _lookup.GetEntitiesInRange<FlammableComponent>(_transform.GetMapCoordinates(uid), spread.Radius);

                foreach (var target in targets)
                {
                    if (!_random.Prob(spread.Prob))
                        continue;

                    _flammable.Ignite(target, uid);

                    var cooldown = _random.NextFloat(spread.SpreadCooldownMin, spread.SpreadCooldownMax);
                    spread.NextSpreadTime = _gameTiming.CurTime + TimeSpan.FromSeconds(cooldown);
                }
            }
        }
    }
}
