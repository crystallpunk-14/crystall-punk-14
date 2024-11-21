using Robust.Shared.Timing;

namespace Content.Shared._CP14.Temperature;

public abstract partial class CP14SharedFireSpreadSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14FireSpreadComponent, OnFireChangedEvent>(OnFireChangedSpread);
        SubscribeLocalEvent<CP14DespawnOnExtinguishComponent, OnFireChangedEvent>(OnFireChangedDespawn);
    }

    private void OnFireChangedDespawn(Entity<CP14DespawnOnExtinguishComponent> ent, ref OnFireChangedEvent args)
    {
        if (!args.OnFire)
            QueueDel(ent);
    }

    private void OnFireChangedSpread(Entity<CP14FireSpreadComponent> ent, ref OnFireChangedEvent args)
    {
        if (args.OnFire)
        {
            EnsureComp<CP14ActiveFireSpreadingComponent>(ent);
        }
        else
        {
            if (HasComp<CP14ActiveFireSpreadingComponent>(ent))
                RemCompDeferred<CP14ActiveFireSpreadingComponent>(ent);
        }

        ent.Comp.NextSpreadTime = _gameTiming.CurTime + TimeSpan.FromSeconds(ent.Comp.SpreadCooldownMax);
    }
}

/// <summary>
/// Raised whenever an FlammableComponent OnFire is Changed
/// </summary>
[ByRefEvent]
public readonly record struct OnFireChangedEvent(bool OnFire)
{
    public readonly bool OnFire = OnFire;
}
