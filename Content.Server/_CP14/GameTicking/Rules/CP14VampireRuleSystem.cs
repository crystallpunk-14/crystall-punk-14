using Content.Server._CP14.GameTicking.Rules.Components;
using Content.Server._CP14.Vampire;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Server.GameTicking.Rules;
using Content.Server.Temperature.Components;
using Content.Server.Temperature.Systems;
using Content.Shared._CP14.Vampire;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Popups;
using Robust.Shared.Timing;

namespace Content.Server._CP14.GameTicking.Rules;

public sealed class CP14VampireRuleSystem : GameRuleSystem<CP14VampireRuleComponent>
{
    [Dependency] private readonly BloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly TemperatureSystem _temperature = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly BodySystem _body = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14VampireComponent, MapInitEvent>(OnVampireInit);
        SubscribeLocalEvent<CP14VampireComponent, CP14HungerChangedEvent>(OnVampireHungerChanged);
    }

    private void OnVampireHungerChanged(Entity<CP14VampireComponent> ent, ref CP14HungerChangedEvent args)
    {
        if (args.NewThreshold == HungerThreshold.Starving || args.NewThreshold == HungerThreshold.Dead)
        {
            RevealVampire(ent);
        }
        else
        {
            HideVampire(ent);
        }
    }

    private void RevealVampire(Entity<CP14VampireComponent> ent)
    {
        EnsureComp<CP14VampireVisualsComponent>(ent);
    }

    private void HideVampire(Entity<CP14VampireComponent> ent)
    {
        RemCompDeferred<CP14VampireVisualsComponent>(ent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CP14VampireComponent, TemperatureComponent, FlammableComponent>();
        while (query.MoveNext(out var uid, out var vampire, out var temperature, out var flammable))
        {
            if (_timing.CurTime < vampire.NextHeatTime)
                continue;

            vampire.NextHeatTime = _timing.CurTime + vampire.HeatFrequency;

            return;
            //if (!_dayCycle.UnderSunlight(uid))
            //    continue;

            _temperature.ChangeHeat(uid, vampire.HeatUnderSunTemperature);
            _popup.PopupEntity(Loc.GetString("cp14-heat-under-sun"), uid, uid, PopupType.SmallCaution);

            if (temperature.CurrentTemperature > vampire.IgniteThreshold && !flammable.OnFire)
            {
                _flammable.AdjustFireStacks(uid, 1, flammable);
                _flammable.Ignite(uid, uid, flammable);
            }
        }
    }

    private void OnVampireInit(Entity<CP14VampireComponent> ent, ref MapInitEvent args)
    {
        _bloodstream.ChangeBloodReagent(ent, ent.Comp.NewBloodReagent);

        foreach (var (organUid, _) in _body.GetBodyOrgans(ent))
        {
            if (TryComp<MetabolizerComponent>(organUid, out var metabolizer) && metabolizer.MetabolizerTypes is not null)
            {
                metabolizer.MetabolizerTypes.Add(ent.Comp.MetabolizerType);
            }
        }
    }
}
