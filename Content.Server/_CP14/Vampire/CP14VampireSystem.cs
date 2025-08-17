using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Server.Temperature.Components;
using Content.Server.Temperature.Systems;
using Content.Shared._CP14.DayCycle;
using Content.Shared._CP14.Vampire;
using Content.Shared._CP14.Vampire.Components;
using Content.Shared.Popups;
using Robust.Shared.Timing;

namespace Content.Server._CP14.Vampire;

public sealed class CP14VampireSystem : CP14SharedVampireSystem
{
    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly TemperatureSystem _temperature = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly CP14DayCycleSystem _dayCycle = default!;

    protected override void OnVampireInit(Entity<CP14VampireComponent> ent, ref MapInitEvent args)
    {
        base.OnVampireInit(ent, ref args);

        //Metabolism
        foreach (var (organUid, _) in _body.GetBodyOrgans(ent))
        {
            if (TryComp<MetabolizerComponent>(organUid, out var metabolizer) && metabolizer.MetabolizerTypes is not null)
            {
                metabolizer.MetabolizerTypes.Add(ent.Comp.MetabolizerType);
            }
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CP14VampireComponent, CP14VampireVisualsComponent, TemperatureComponent, FlammableComponent>();
        while (query.MoveNext(out var uid, out var vampire, out var visuals, out var temperature, out var flammable))
        {
            if (_timing.CurTime < vampire.NextHeatTime)
                continue;

            vampire.NextHeatTime = _timing.CurTime + vampire.HeatFrequency;

            if (!_dayCycle.UnderSunlight(uid))
                continue;

            _temperature.ChangeHeat(uid, vampire.HeatUnderSunTemperature);
            _popup.PopupEntity(Loc.GetString("cp14-heat-under-sun"), uid, uid, PopupType.SmallCaution);

            if (temperature.CurrentTemperature > vampire.IgniteThreshold && !flammable.OnFire)
            {
                _flammable.AdjustFireStacks(uid, 1, flammable);
                _flammable.Ignite(uid, uid, flammable);
            }
        }
    }
}
