using Content.Server._CP14.MagicEnergy.Components;
using Content.Shared._CP14.DayCycle;
using Content.Shared._CP14.MagicEnergy.Components;
using Content.Shared.Damage;

namespace Content.Server._CP14.MagicEnergy;

public partial class CP14MagicEnergySystem
{
    [Dependency] private readonly CP14SharedDayCycleSystem _dayCycle = default!;

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

            ChangeEnergy(ent, modifier * dict.Value, true);
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

            draw.NextUpdateTime = _gameTiming.CurTime + TimeSpan.FromSeconds(draw.Delay);

            ChangeEnergy(uid, magicContainer, draw.Energy, draw.Safe);
        }

        var query2 = EntityQueryEnumerator<CP14MagicEnergyPhotosynthesisComponent, CP14MagicEnergyContainerComponent>();
        while (query2.MoveNext(out var uid, out var draw, out var magicContainer))
        {
            if (draw.NextUpdateTime >= _gameTiming.CurTime)
                continue;

            draw.NextUpdateTime = _gameTiming.CurTime + TimeSpan.FromSeconds(draw.Delay);

            ChangeEnergy(uid, magicContainer, _dayCycle.TryDaylightThere(uid) ? draw.DaylightEnergy : draw.DarknessEnergy, true);
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

            if (!_magicSlot.TryGetEnergyCrystalFromSlot(uid, out var energyEnt, out var energyComp))
                continue;

            ChangeEnergy(energyEnt.Value, energyComp, draw.Energy, draw.Safe);
        }
    }
}
