using Content.Server._CP14.GameTicking.Rules.Components;
using Content.Server.Chat.Systems;
using Content.Server.Station.Components;
using Content.Server.StationEvents.Events;
using Content.Shared._CP14.DayCycle;
using Robust.Shared.Player;

namespace Content.Server._CP14.GameTicking.Rules;

public sealed class CP14BloodMoonRule : StationEventSystem<CP14BloodMoonRuleComponent>
{
    [Dependency] private readonly ChatSystem _chatSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14StartDayEvent>(OnStartDay);
        SubscribeLocalEvent<CP14StartNightEvent>(OnStartNight);
    }

    private void OnStartDay(CP14StartDayEvent ev)
    {
        if (!HasComp<BecomesStationComponent>(ev.Map))
            return;

        var query = QueryActiveRules();
        while (query.MoveNext(out var uid, out _, out var comp, out _))
        {
            if (!comp.Announced)
            {
                comp.Announced = true;

                Filter allPlayersInGame = Filter.Empty().AddWhere(GameTicker.UserHasJoinedGame);
                _chatSystem.DispatchFilteredAnnouncement(allPlayersInGame, Loc.GetString(comp.StartAnnouncement), playSound: false, sender: Loc.GetString("cp14-announcement-gamemaster"), colorOverride: comp.AnnouncementColor);
            }
        }
    }

    private void OnStartNight(CP14StartNightEvent ev)
    {
        if (!HasComp<BecomesStationComponent>(ev.Map))
            return;

        var query = QueryActiveRules();
        while (query.MoveNext(out var uid, out _, out var comp, out _))
        {
            if (!comp.Announced)
                continue;

            GameTicker.AddGameRule(comp.CurseRule);
            ForceEndSelf(uid);
        }
    }
}
