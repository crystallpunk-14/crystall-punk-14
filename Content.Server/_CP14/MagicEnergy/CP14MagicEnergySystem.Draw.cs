using Content.Server._CP14.MagicEnergy.Components;
using Content.Shared._CP14.MagicEnergy.Components;
using Content.Shared.Damage;
using Robust.Shared.Map.Components;

namespace Content.Server._CP14.MagicEnergy;

public partial class CP14MagicEnergySystem
{
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

            ChangeEnergy(ent, modifier * dict.Value, out _, out _, safe: true);
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

            ChangeEnergy(uid, draw.Energy, out _, out _, magicContainer, draw.Safe);
        }

        var query2 = EntityQueryEnumerator<CP14MagicEnergyPhotosynthesisComponent, CP14MagicEnergyContainerComponent>();
        while (query2.MoveNext(out var uid, out var draw, out var magicContainer))
        {
            if (draw.NextUpdateTime >= _gameTiming.CurTime)
                continue;

            draw.NextUpdateTime = _gameTiming.CurTime + TimeSpan.FromSeconds(draw.Delay);

            var daylight = false;

            //if (TryComp<MapLightComponent>(Transform(uid).MapUid, out var mapLight))
            //{
            //    var color = mapLight.AmbientLightColor;
            //    var medium = (color.R + color.G + color.B) / 3f;
            //
            //    if (medium > draw.LightThreshold)
            //        daylight = true;
            //}

            ChangeEnergy(uid, daylight ? draw.DaylightEnergy : draw.DarknessEnergy, out _, out _, magicContainer, true);
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

            ChangeEnergy(energyEnt.Value, draw.Energy, out _, out _, energyComp, draw.Safe);
        }
    }
}
