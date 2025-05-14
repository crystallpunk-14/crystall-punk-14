using Content.Server._CP14.GameTicking.Rules.Components;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Shared._CP14.DayCycle;
using Content.Shared.GameTicking.Components;
using Robust.Shared.Player;

namespace Content.Server._CP14.GameTicking.Rules;

public sealed class CP14BloodMoonRule : GameRuleSystem<CP14BloodMoonRuleComponent>
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
        var query = QueryActiveRules();
        while (query.MoveNext(out var uid, out _, out var comp, out _))
        {
            if (!comp.Announed)
            {
                comp.Announed = true;

                Filter allPlayersInGame = Filter.Empty().AddWhere(GameTicker.UserHasJoinedGame);
                _chatSystem.DispatchFilteredAnnouncement(allPlayersInGame, Loc.GetString(comp.StartAnnouncement), playSound: false, colorOverride: comp.AnnouncementColor);
            }
        }
    }

    private void OnStartNight(CP14StartNightEvent ev)
    {
        var query = QueryActiveRules();
        while (query.MoveNext(out var uid, out _, out var comp, out _))
        {
            if (!comp.Announed)
                continue;

            GameTicker.AddGameRule(comp.CurseRule);
            ForceEndSelf(uid);
        }
    }
}
