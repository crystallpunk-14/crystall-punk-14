using Content.Server._CP14.GameTicking.Rules.Components;
using Content.Server.Antag;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking.Rules;
using Content.Server.Popups;
using Content.Server.Station.Components;
using Content.Server.Stunnable;
using Content.Shared._CP14.BloodMoon;
using Content.Shared._CP14.DayCycle;
using Content.Shared.Examine;
using Content.Shared.GameTicking.Components;
using Content.Shared.Mobs;
using Content.Shared.Popups;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;

namespace Content.Server._CP14.GameTicking.Rules;

public sealed class CP14BloodMoonCurseRule : GameRuleSystem<CP14BloodMoonCurseRuleComponent>
{
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14StartDayEvent>(OnStartDay);
        SubscribeLocalEvent<CP14BloodMoonCurseRuleComponent, AfterAntagEntitySelectedEvent>(AfterAntagEntitySelected);
        SubscribeLocalEvent<CP14BloodMoonCurseComponent, ExaminedEvent>(CurseExamined);
        SubscribeLocalEvent<CP14BloodMoonCurseComponent, MobStateChangedEvent>(OnDead);
    }

    private void OnDead(Entity<CP14BloodMoonCurseComponent> ent, ref MobStateChangedEvent args)
    {
        ClearCurse((ent.Owner, ent.Comp));
    }

    private void CurseExamined(Entity<CP14BloodMoonCurseComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("cp14-bloodmoon-curse-examined"));
    }

    private void AfterAntagEntitySelected(Entity<CP14BloodMoonCurseRuleComponent> ent, ref AfterAntagEntitySelectedEvent args)
    {
        SpawnAttachedTo(ent.Comp.CurseEffect, Transform(args.EntityUid).Coordinates);
        var curseComp = EnsureComp<CP14BloodMoonCurseComponent>(args.EntityUid);
        var effect = SpawnAttachedTo(curseComp.CurseEffect, Transform(args.EntityUid).Coordinates);
        curseComp.SpawnedEffect = effect;
        curseComp.CurseRule = ent;
        _transform.SetParent(effect, args.EntityUid);
    }

    protected override void Started(EntityUid uid,
        CP14BloodMoonCurseRuleComponent component,
        GameRuleComponent gameRule,
        GameRuleStartedEvent args)
    {
        Filter allPlayersInGame = Filter.Empty().AddWhere(GameTicker.UserHasJoinedGame);
        _chatSystem.DispatchFilteredAnnouncement(allPlayersInGame, Loc.GetString(component.StartAnnouncement), colorOverride: component.AnnouncementColor);

        _audio.PlayGlobal(component.GlobalSound, allPlayersInGame, true);
    }

    protected override void Ended(EntityUid uid,
        CP14BloodMoonCurseRuleComponent component,
        GameRuleComponent gameRule,
        GameRuleEndedEvent args)
    {
        Filter allPlayersInGame = Filter.Empty().AddWhere(GameTicker.UserHasJoinedGame);
        _chatSystem.DispatchFilteredAnnouncement(allPlayersInGame, Loc.GetString(component.EndAnnouncement), colorOverride: component.AnnouncementColor);

        var aliveAntags = _antag.GetAliveAntags(uid);

        foreach (var antag in aliveAntags)
        {
            SpawnAttachedTo(component.CurseEffect, Transform(antag).Coordinates);
            ClearCurse(antag);
        }
    }

    private void OnStartDay(CP14StartDayEvent ev)
    {
        if (!HasComp<BecomesStationComponent>(ev.Map))
            return;

        var query = QueryActiveRules();
        while (query.MoveNext(out var uid, out _, out var comp, out _))
        {
            ForceEndSelf(uid);
        }
    }

    private void ClearCurse(Entity<CP14BloodMoonCurseComponent?> ent)
    {
        if (!Resolve(ent.Owner, ref ent.Comp, false))
            return;

        _stun.TryParalyze(ent, ent.Comp.EndStunDuration, true);
        _popup.PopupEntity(Loc.GetString("cp14-bloodmoon-curse-removed"), ent, PopupType.SmallCaution);
        if (TryComp<CP14BloodMoonCurseComponent>(ent, out var curseComp))
        {
            QueueDel(curseComp.SpawnedEffect);
            RemCompDeferred<CP14BloodMoonCurseComponent>(ent);
        }
    }
}
