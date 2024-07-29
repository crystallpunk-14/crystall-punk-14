using System.Numerics;
using Content.Server._CP14.MagicEnergy.Components;
using Content.Shared._CP14.MagicEnergy.Components;

namespace Content.Server._CP14.MagicEnergy;

public partial class CP14MagicEnergySystem
{
    private void InitializeDraw()
    {
        SubscribeLocalEvent<CP14MagicEnergyDrawComponent, MapInitEvent>(OnDrawMapInit);
        SubscribeLocalEvent<CP14RandomAuraNodeComponent, MapInitEvent>(OnRandomRangeMapInit);
    }

    private void OnRandomRangeMapInit(Entity<CP14RandomAuraNodeComponent> random, ref MapInitEvent args)
    {
        if (!TryComp<CP14AuraNodeComponent>(random, out var draw))
            return;

        draw.Energy = _random.NextFloat(random.Comp.MinDraw, random.Comp.MaxDraw);
        draw.Range = _random.NextFloat(random.Comp.MinRange, random.Comp.MaxRange);
    }

    private void OnDrawMapInit(Entity<CP14MagicEnergyDrawComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.NextUpdateTime = _gameTiming.CurTime + TimeSpan.FromSeconds(ent.Comp.Delay);
    }

    private void UpdateDraw(float frameTime)
    {
        UpdateEnergyContainer();
        UpdateEnergyCrystalSlot();
        UpdateEnergyRadiusDraw();
    }

    private void UpdateEnergyContainer()
    {
        var query = EntityQueryEnumerator<CP14MagicEnergyDrawComponent, CP14MagicEnergyContainerComponent>();
        while (query.MoveNext(out var uid, out var draw, out var magicContainer))
        {
            if (draw.NextUpdateTime >= _gameTiming.CurTime)
                continue;

            draw.NextUpdateTime = _gameTiming.CurTime + TimeSpan.FromSeconds(draw.Delay);

            ChangeEnergy(uid, magicContainer, draw.Energy, safe: draw.Safe);
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

    private void UpdateEnergyRadiusDraw()
    {
        var query = EntityQueryEnumerator<CP14AuraNodeComponent>();
        while (query.MoveNext(out var uid, out var draw))
        {
            if (!draw.Enable)
                continue;

            if (draw.NextUpdateTime >= _gameTiming.CurTime)
                continue;

            draw.NextUpdateTime = _gameTiming.CurTime + TimeSpan.FromSeconds(draw.Delay);

            var containers = _lookup.GetEntitiesInRange<CP14MagicEnergyContainerComponent>(Transform(uid).Coordinates, draw.Range);
            foreach (var container in containers)
            {
                var distance = Vector2.Distance(_transform.GetWorldPosition(uid), _transform.GetWorldPosition(container));
                var energyDraw = draw.Energy * (1 - distance / draw.Range);

                ChangeEnergy(container, container.Comp, energyDraw, true);
            }
        }
    }
}
