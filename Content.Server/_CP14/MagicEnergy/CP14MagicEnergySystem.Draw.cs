using Content.Server._CP14.MagicEnergy.Components;
using Content.Shared._CP14.DayCycle;
using Content.Shared._CP14.MagicEnergy.Components;
using Content.Shared.Damage;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Map.Components;

namespace Content.Server._CP14.MagicEnergy;

public partial class CP14MagicEnergySystem
{
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly CP14DayCycleSystem _dayCycle = default!;

    private void InitializeDraw()
    {
        SubscribeLocalEvent<CP14MagicEnergyDrawComponent, MapInitEvent>(OnDrawMapInit);
        SubscribeLocalEvent<CP14MagicEnergyFromDamageComponent, DamageChangedEvent>(OnDamageChanged);
    }

    private void OnDamageChanged(Entity<CP14MagicEnergyFromDamageComponent> ent, ref DamageChangedEvent args)
    {
        if (args.DamageDelta is null || !args.DamageIncreased)
            return;

        foreach (var dict in args.DamageDelta.DamageDict)
        {
            if (dict.Value <= 0)
                continue;

            if (!ent.Comp.Damage.TryGetValue(dict.Key, out var modifier))
                continue;

            ChangeEnergy(ent.Owner, modifier * dict.Value, out _, out _, safe: true);
        }
    }

    private void OnDrawMapInit(Entity<CP14MagicEnergyDrawComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.NextUpdateTime = _gameTiming.CurTime + TimeSpan.FromSeconds(ent.Comp.Delay);
    }

    private void UpdateDraw(float frameTime)
    {
        UpdateEnergyContainer();
        UpdateEnergyCrystalSlot();
    }

    private void UpdateEnergyContainer()
    {
        var query = EntityQueryEnumerator<CP14MagicEnergyDrawComponent, CP14MagicEnergyContainerComponent>();
        while (query.MoveNext(out var uid, out var draw, out var magicContainer))
        {
            if (!draw.Enable)
                continue;

            if (draw.NextUpdateTime >= _gameTiming.CurTime)
                continue;

            if (TryComp<MobStateComponent>(uid, out var mobState) && !_mobState.IsAlive(uid, mobState))
                continue;

            draw.NextUpdateTime += TimeSpan.FromSeconds(draw.Delay);

            ChangeEnergy((uid, magicContainer), draw.Energy, out _, out _, draw.Safe);
        }

        var query2 = EntityQueryEnumerator<CP14MagicEnergyPhotosynthesisComponent, CP14MagicEnergyContainerComponent>();
        while (query2.MoveNext(out var uid, out var draw, out var magicContainer))
        {
            if (draw.NextUpdateTime >= _gameTiming.CurTime)
                continue;

            if (TryComp<MobStateComponent>(uid, out var mobState) && !_mobState.IsAlive(uid, mobState))
                continue;

            draw.NextUpdateTime = _gameTiming.CurTime + TimeSpan.FromSeconds(draw.Delay);

            if (!_dayCycle.UnderSunlight(uid))
                continue;

            ChangeEnergy((uid, magicContainer), daylight ? draw.DaylightEnergy : draw.DarknessEnergy, out _, out _, true);
        }
    }

    private void UpdateEnergyCrystalSlot()
    {
        var query = EntityQueryEnumerator<CP14MagicEnergyDrawComponent, CP14MagicEnergyCrystalSlotComponent>();
        while (query.MoveNext(out var uid, out var draw, out var slot))
        {
            if (!draw.Enable)
                continue;

            if (draw.NextUpdateTime >= _gameTiming.CurTime)
                continue;

            draw.NextUpdateTime = _gameTiming.CurTime + TimeSpan.FromSeconds(draw.Delay);

            if (!_magicSlot.TryGetEnergyCrystalFromSlot(uid, out var energyEnt))
                continue;

            ChangeEnergy((energyEnt.Value, energyEnt.Value), draw.Energy, out _, out _, draw.Safe);
        }
    }
}
