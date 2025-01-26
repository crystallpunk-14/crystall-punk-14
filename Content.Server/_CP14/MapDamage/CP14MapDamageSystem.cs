using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Timing;

namespace Content.Server._CP14.MapDamage;

public sealed partial class CP14MapDamageSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly StaminaSystem _stamina = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    private EntityQuery<CP14MapDamageComponent> _mapQuery;
    public override void Initialize()
    {
        base.Initialize();

        _mapQuery = GetEntityQuery<CP14MapDamageComponent>();

        SubscribeLocalEvent<CP14DamageableByMapComponent, EntParentChangedMessage>(OnParentChanged);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CP14DamageableByMapComponent, DamageableComponent, MobStateComponent>();
        while (query.MoveNext(out var uid, out var damage, out var damageable, out var mobState))
        {
            if (!damage.Enabled)
                continue;

            if (damage.NextDamageTime > _timing.CurTime)
                continue;

            damage.NextDamageTime = _timing.CurTime + TimeSpan.FromSeconds(2f);

            if (damage.CurrentMap is null)
                continue;

            if (!_mobState.IsAlive(uid, mobState))
                continue;

            if (!_mapQuery.TryComp(damage.CurrentMap.Value, out var mapDamage))
                continue;

            _damageable.TryChangeDamage(uid, mapDamage.Damage, damageable: damageable);
            _stamina.TakeStaminaDamage(uid, mapDamage.StaminaDamage);
        }
    }

    private void OnParentChanged(Entity<CP14DamageableByMapComponent> ent, ref EntParentChangedMessage args)
    {
        DisableDamage(ent);
        if (args.OldParent == null || TerminatingOrDeleted(ent))
            return;

        var newParent = _transform.GetParentUid(ent);
        if (!TryComp<CP14MapDamageComponent>(newParent, out var mapDamage))
            return;

        EnableDamage(ent, (newParent, mapDamage));
    }

    private void DisableDamage(Entity<CP14DamageableByMapComponent> ent)
    {
        if (!ent.Comp.Enabled)
            return;

        ent.Comp.Enabled = false;
        ent.Comp.CurrentMap = null;
        Dirty(ent);
    }

    private void EnableDamage(Entity<CP14DamageableByMapComponent> ent, Entity<CP14MapDamageComponent> map)
    {
        if (ent.Comp.Enabled)
            return;

        ent.Comp.Enabled = true;
        ent.Comp.CurrentMap = map;
        Dirty(ent);
    }
}
