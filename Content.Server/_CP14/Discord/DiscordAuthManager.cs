using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Content.Shared._CP14.Discord;
using Content.Shared.CCVar;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Server._CP14.Discord;

public sealed class DiscordAuthManager
{
    [Dependency] private readonly IServerNetManager _netMgr = default!;
    [Dependency] private readonly IPlayerManager _playerMgr = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    private ISawmill _sawmill = default!;
    private readonly HttpClient _httpClient = new();
    private bool _enabled = false;
    private string _apiUrl = string.Empty;
    private string _apiKey = string.Empty;

    public event EventHandler<ICommonSession>? PlayerVerified;

    public void Initialize()
    {
        _sawmill = Logger.GetSawmill("discordAuth");

        _cfg.OnValueChanged(CCVars.DiscordAuthEnabled, v => _enabled = v, true);
        _cfg.OnValueChanged(CCVars.DiscordAuthUrl, v => _apiUrl = v, true);
        _cfg.OnValueChanged(CCVars.DiscordAuthToken, v => _apiKey = v, true);

        _netMgr.RegisterNetMessage<MsgDiscordAuthRequired>();
        _netMgr.RegisterNetMessage<MsgDiscordAuthCheck>(OnAuthCheck);

        _playerMgr.PlayerStatusChanged += OnPlayerStatusChanged;
    }

    private async void OnAuthCheck(MsgDiscordAuthCheck msg)
    {
        var verified = await IsVerified(msg.MsgChannel.UserId);
        if (!verified)
            return;

        var session = _playerMgr.GetSessionById(msg.MsgChannel.UserId);
        PlayerVerified?.Invoke(this, session);
    }

    private async void OnPlayerStatusChanged(object? sender, SessionStatusEventArgs args)
    {
        if (args.NewStatus != SessionStatus.Connected)
            return;

        if (!_enabled)
        {
            PlayerVerified?.Invoke(this, args.Session);
            return;
        }

        if (args.NewStatus == SessionStatus.Connected)
        {
            var verified = await IsVerified(args.Session.UserId);
            if (verified)
            {
                PlayerVerified?.Invoke(this, args.Session);
                return;
            }

            var message = new MsgDiscordAuthRequired();
            args.Session.Channel.SendMessage(message);
        }
    }

    public async Task<bool> IsVerified(NetUserId userId, CancellationToken cancel = default)
    {
        _sawmill.Debug($"Player {userId} check Discord verification");

        var requestUrl = $"{_apiUrl}?method=ss14&id={userId}";
        var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        var response = await _httpClient.SendAsync(request, cancel);

        _sawmill.Debug($"{await response.Content.ReadAsStringAsync(cancel)}");
        _sawmill.Debug($"{(int) response.StatusCode}");
        return response.StatusCode == HttpStatusCode.OK;
    }
}
