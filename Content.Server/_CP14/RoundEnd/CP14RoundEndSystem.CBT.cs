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
    private readonly TimeSpan _updateFrequency = TimeSpan.FromSeconds(45f);

    private bool _enabled;

    private void InitCbt()
    {
        _enabled = _configManager.GetCVar(CCVars.CP14ClosedBetaTest);
        _configManager.OnValueChanged(CCVars.CP14ClosedBetaTest, _ => { _enabled = _configManager.GetCVar(CCVars.CP14ClosedBetaTest); }, true);
    }

    // Вы можете сказать: Эд, ты ебанулся? Это же лютый щиткод!
    // И я вам отвечу: Да. Но сама система ограничения времени работы сервера - временная штука на этап разработки, которая будет удалена. Мне просто лень каждый раз запускать и выключать сервер ручками.
    private void UpdateCbt(float _)
    {
        if (!_enabled)
            return;

        if (_nextUpdateTime > _timing.CurTime)
            return;

        _nextUpdateTime = _timing.CurTime + _updateFrequency;

        DateTime nowMoscow = DateTime.UtcNow.AddHours(3);

        //Disable any round timers
        if (nowMoscow.Hour is < 18 or > 20)
        {
            if (_ticker.RunLevel == GameRunLevel.InRound)
                _roundEnd.EndRound();

            if (!_ticker.Paused)
                _ticker.TogglePause();
        }
        else
        {
            if (_ticker.Paused)
                _ticker.TogglePause();
        }

        if (nowMoscow.Hour == 20 && nowMoscow.Minute == 45)
        {
            _chatSystem.DispatchGlobalAnnouncement("ВНИМАНИЕ: Сервер автоматически завершит раунд через 15 минут", announcementSound: new SoundPathSpecifier("/Audio/Effects/beep1.ogg"), sender: "Сервер");
        }
        if (nowMoscow.Hour == 21 && nowMoscow.Minute == 02)
        {
            _consoleHost.ExecuteCommand("golobby");
            _consoleHost.ExecuteCommand("set-motd Плейтест на сегодня уже закончен. Следующий запуск в 18:00 МСК.");
        }
    }
}
