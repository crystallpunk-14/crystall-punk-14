using Content.Server.GameTicking;
using Content.Shared.CCVar;
using Robust.Server;
using Robust.Shared.Audio;
using Robust.Shared.Console;

namespace Content.Server._CP14.RoundEnd;

public sealed partial class CP14RoundEndSystem
{
    [Dependency] private readonly IConsoleHost _consoleHost = default!;
    [Dependency] private readonly GameTicker _ticker = default!;
    [Dependency] private readonly IBaseServer _baseServer = default!;

    private TimeSpan _nextUpdateTime = TimeSpan.Zero;
    private readonly TimeSpan _updateFrequency = TimeSpan.FromSeconds(50f);

    private bool _enabled;

    private void InitCbt()
    {
        _enabled = _cfg.GetCVar(CCVars.CP14ClosedBetaTest);
        _cfg.OnValueChanged(CCVars.CP14ClosedBetaTest,
            _ => { _enabled = _cfg.GetCVar(CCVars.CP14ClosedBetaTest); },
            true);
    }

    // Вы можете сказать: Эд, ты ебанулся? Это же лютый щиткод!
    // И я вам отвечу: Да. Но сама система ограничения времени работы сервера - временная штука на этап разработки, которая будет удалена.
    // Мне просто лень каждый раз запускать и выключать сервер ручками.
    private void UpdateCbt(float _)
    {
        if (!_enabled || _timing.CurTime < _nextUpdateTime)
            return;

        _nextUpdateTime = _timing.CurTime + _updateFrequency;
        var now = DateTime.UtcNow.AddHours(3); // Moscow time

        OpenSaturdayRule(now);
        LanguageRule(now);
        LimitPlaytimeRule(now);
        ApplyAnnouncements(now);
    }

    private void OpenSaturdayRule(DateTime now)
    {
        var curWhitelist = _cfg.GetCVar(CCVars.WhitelistEnabled);
        var isOpenWeened = now.DayOfWeek is DayOfWeek.Saturday;

        if (isOpenWeened && curWhitelist)
        {
            _cfg.SetCVar(CCVars.WhitelistEnabled, false);
        }
        else if (!isOpenWeened && !curWhitelist)
        {
            _cfg.SetCVar(CCVars.WhitelistEnabled, true);
        }
    }

    private void LanguageRule(DateTime now)
    {
        var curLang = _cfg.GetCVar(CCVars.Language);

        var ruHalfDay = now.Hour < 19 && now.Hour >= 9;


        if (ruHalfDay && curLang != "ru-RU")
        {
            _cfg.SetCVar(CCVars.Language, "ru-RU");

            _chatSystem.DispatchGlobalAnnouncement(
                "WARNING: The server changes its language to Russian. For the changes to apply to your device, reconnect to the server.",
                announcementSound: new SoundPathSpecifier("/Audio/Effects/beep1.ogg"),
                sender: "Server"
            );
        }
        else if (!ruHalfDay && curLang != "en-US")
        {
            _cfg.SetCVar(CCVars.Language, "en-US");

            _chatSystem.DispatchGlobalAnnouncement(
                "WARNING: The server changes its language to English. For the changes to apply to your device, reconnect to the server.",
                announcementSound: new SoundPathSpecifier("/Audio/Effects/beep1.ogg"),
                sender: "Server"
            );
        }
    }

    private void LimitPlaytimeRule(DateTime now)
    {
        var playtime = (now.Hour is >= 15 and < 19) || (now.Hour is >= 20 and < 24);

        if (playtime)
        {
            if (_ticker.Paused)
                _ticker.TogglePause();
        }
        else
        {
            if (_ticker.RunLevel == GameRunLevel.InRound)
                _roundEnd.EndRound();

            if (!_ticker.Paused)
                _ticker.TogglePause();
        }
    }

    private void ApplyAnnouncements(DateTime now)
    {
        var timeMap = new (int Hour, int Minute, Action Action)[]
        {
            (18, 45, () =>
            {
                _chatSystem.DispatchGlobalAnnouncement(
                    Loc.GetString("cp14-cbt-close-15m"),
                    announcementSound: new SoundPathSpecifier("/Audio/Effects/beep1.ogg"),
                    sender: "Server"
                );
            }),
            (19, 2, () =>
            {
                _baseServer.Shutdown("Русский ОБТ подошел к концу. Следующие 3 часа будет английский ОБТ. Просьба не мешать англоязычным ребятам играть в свое время :)");
            }),
            (23, 45, () =>
            {
                _chatSystem.DispatchGlobalAnnouncement(
                    Loc.GetString("cp14-cbt-close-15m"),
                    announcementSound: new SoundPathSpecifier("/Audio/Effects/beep1.ogg"),
                    sender: "Server"
                );
            }),
            (00, 2, () =>
            {
                _consoleHost.ExecuteCommand("golobby");
            }),
        };

        foreach (var (hour, minute, action) in timeMap)
        {
            if (now.Hour == hour && now.Minute == minute)
                action.Invoke();
        }
    }
}
