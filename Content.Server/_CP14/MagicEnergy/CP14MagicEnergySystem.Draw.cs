using Content.Server._CP14.MagicEnergy.Components;
using Content.Shared._CP14.MagicEnergy.Components;

namespace Content.Server._CP14.MagicEnergy;

public partial class CP14MagicEnergySystem
{
    private void InitializeDraw()
    {
        SubscribeLocalEvent<CP14MagicEnergyDrawComponent, MapInitEvent>(OnDrawMapInit);
    }

    private void UpdateDraw(float frameTime)
    {
        var query = EntityQueryEnumerator<CP14MagicEnergyDrawComponent, CP14MagicEnergyContainerComponent>();
        while (query.MoveNext(out var uid, out var draw, out var magicContainer))
        {
            if (draw.NextUpdateTime >= _gameTiming.CurTime)
                continue;

            draw.NextUpdateTime = _gameTiming.CurTime + TimeSpan.FromSeconds(draw.Delay);

            ChangeEnergy(uid, magicContainer, draw.Energy, safe: draw.Safe);
        }

        var query2 = EntityQueryEnumerator<CP14MagicEnergyDrawComponent, CP14MagicEnergyCrystalSlotComponent>();
        while (query2.MoveNext(out var uid, out var draw, out var slot))
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

    private void OnDrawMapInit(Entity<CP14MagicEnergyDrawComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.NextUpdateTime = _gameTiming.CurTime + TimeSpan.FromSeconds(ent.Comp.Delay);
    }
}
