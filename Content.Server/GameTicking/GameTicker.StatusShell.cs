using System.Linq;
using System.Text.Json.Nodes;
using Content.Server._CP14.JoinQueue;
using Content.Shared.CCVar;
using Content.Shared.GameTicking;
using Robust.Server.ServerStatus;
using Robust.Shared.Configuration;

namespace Content.Server.GameTicking
{
    public sealed partial class GameTicker
    {
        /// <summary>
        ///     Used for thread safety, given <see cref="IStatusHost.OnStatusRequest"/> is called from another thread.
        /// </summary>
        private readonly object _statusShellLock = new();

        /// <summary>
        ///     Round start time in UTC, for status shell purposes.
        /// </summary>
        [ViewVariables]
        private DateTime _roundStartDateTime;

        /// <summary>
        ///     For access to CVars in status responses.
        /// </summary>
        [Dependency] private readonly IConfigurationManager _cfg = default!;
        /// <summary>
        ///     For access to the round ID in status responses.
        /// </summary>
        [Dependency] private readonly SharedGameTicker _gameTicker = default!;

        /// <summary>
        ///     For getting actual players count
        /// </summary>
        [Dependency] private readonly JoinQueueManager _joinQueue = default!;
        //CP14 - JoinQueue
        private void InitializeStatusShell()
        {
            IoCManager.Resolve<IStatusHost>().OnStatusRequest += GetStatusResponse;
        }

        private void GetStatusResponse(JsonNode jObject)
        {
            var preset = CurrentPreset ?? Preset;

            // This method is raised from another thread, so this better be thread safe!
            lock (_statusShellLock)
            {
                jObject["name"] = _baseServer.ServerName;
                jObject["map"] = _gameMapManager.GetSelectedMap()?.MapName;
                jObject["round_id"] = _gameTicker.RoundId;
                jObject["players"] = _joinQueue.ActualPlayersCount; //CP14 - JoinQueue
                jObject["soft_max_players"] = _cfg.GetCVar(CCVars.SoftMaxPlayers);
                jObject["panic_bunker"] = _cfg.GetCVar(CCVars.PanicBunkerEnabled);
                jObject["run_level"] = (int) _runLevel;
                if (preset != null)
                    jObject["preset"] = Loc.GetString(preset.ModeTitle);
                if (_runLevel >= GameRunLevel.InRound)
                {
                    jObject["round_start_time"] = _roundStartDateTime.ToString("o");
                }
            }
        }
    }
}
