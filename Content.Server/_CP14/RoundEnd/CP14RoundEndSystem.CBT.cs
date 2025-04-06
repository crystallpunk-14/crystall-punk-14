using Content.Server.GameTicking;
using Content.Shared.CCVar;
using Robust.Shared.Audio;
using Robust.Shared.Console;

namespace Content.Server._CP14.RoundEnd;

public sealed partial class CP14RoundEndSystem
{
    [Dependency] private readonly IConsoleHost _consoleHost = default!;
    [Dependency] private readonly GameTicker _ticker = default!;

    private TimeSpan _nextUpdateTime = TimeSpan.Zero;
    private readonly TimeSpan _updateFrequency = TimeSpan.FromSeconds(60f);

    private bool _enabled;

    private string? _currentLang;
    private bool? _whitelistEnabled;

    private void InitCbt()
    {
        _enabled = _configManager.GetCVar(CCVars.CP14ClosedBetaTest);
        _configManager.OnValueChanged(CCVars.CP14ClosedBetaTest,
            _ => { _enabled = _configManager.GetCVar(CCVars.CP14ClosedBetaTest); },
            true);
    }

    // Вы можете сказать: Эд, ты ебанулся? Это же лютый щиткод!
    // И я вам отвечу: Да. Но сама система ограничения времени работы сервера - временная штука на этап разработки, которая будет удалена. Мне просто лень каждый раз запускать и выключать сервер ручками.
    private void UpdateCbt(float _)
    {
        if (!_enabled || _timing.CurTime < _nextUpdateTime)
            return;

        _nextUpdateTime = _timing.CurTime + _updateFrequency;
        var now = DateTime.UtcNow.AddHours(3); // Moscow time

        ApplyLanguageAndWhitelistRules(now);
        ApplyRoundTimers(now);
        ApplyAnnouncements(now);
    }

    private void ApplyLanguageAndWhitelistRules(DateTime now)
    {
        var isEng = EnPlaytestActive(now);

        var desiredLang = isEng ? "en-US" : "ru-RU";
        var desiredWhitelist = !isEng;

        if (_currentLang != desiredLang)
        {
            _configManager.SetCVar(CCVars.Language, desiredLang);
            _currentLang = desiredLang;
        }

        if (_whitelistEnabled != desiredWhitelist)
        {
            _configManager.SetCVar(CCVars.WhitelistEnabled, desiredWhitelist);
            _whitelistEnabled = desiredWhitelist;
        }
    }

    private void ApplyRoundTimers(DateTime now)
    {
        var openedServer = RuPlaytestActive(now) || EnPlaytestActive(now);

        if (openedServer)
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
            (20, 45, () =>
            {
                if (!EnPlaytestActive(now))
                    Announce("ВНИМАНИЕ: Сервер автоматически завершит раунд через 15 минут");
            }),
            (21, 2, () =>
            {
                if (!EnPlaytestActive(now))
                    RunCommand("golobby");
            }),


            (23, 44, () =>
            {
                if (EnPlaytestActive(now))
                    Announce("WARNING: The server will automatically end the round after 15 minutes");
            }),
            (23, 59, () =>
            {
                if (EnPlaytestActive(now))
                    RunCommand("golobby");
            }),
        };

        foreach (var (hour, minute, action) in timeMap)
        {
            if (now.Hour == hour && now.Minute == minute)
                action.Invoke();
        }
    }

    private void Announce(string message)
    {
        _chatSystem.DispatchGlobalAnnouncement(
            message,
            announcementSound: new SoundPathSpecifier("/Audio/Effects/beep1.ogg"),
            sender: "Server"
        );
    }

    private void RunCommand(string command)
    {
        _consoleHost.ExecuteCommand(command);
    }

    private static bool RuPlaytestActive(DateTime now)
    {
        return (now.Hour >= 18 || now.Hour < 21) && now.DayOfWeek != DayOfWeek.Saturday;
    }

    private static bool EnPlaytestActive(DateTime now)
    {
        return now.Hour >= 16 && now.DayOfWeek == DayOfWeek.Saturday;
    }
}
