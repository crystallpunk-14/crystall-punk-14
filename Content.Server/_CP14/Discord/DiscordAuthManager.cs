using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Content.Server.Database;
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
    [Dependency] private readonly IServerDbManager _db = default!;

    private ISawmill _sawmill = default!;
    private readonly HttpClient _httpClient = new();
    private bool _enabled;
    private string _apiUrl = string.Empty;
    private string _apiKey = string.Empty;

    private string _discordGuild = "1221923073759121468"; //CrystallEdge server required

    private HashSet<string> _blockedGuilds = new()
    {
        "1346922008000204891",
        "1186566619858731038",
        "1355279097906855968",
        "1352009516941705216",
        "1359476387190145034",
        "1294276016117911594",
        "1278755078315970620",
    };

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
        if (!verified.Verified)
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
            if (verified.Verified)
            {
                PlayerVerified?.Invoke(this, args.Session);
                return;
            }

            var message = new MsgDiscordAuthRequired();
            message.AuthUrl = await GenerateLink(args.Session.UserId) ?? string.Empty;
            message.ErrorMessage = verified.ErrorMessage;
            args.Session.Channel.SendMessage(message);
        }
    }

    public async Task<AuthData> IsVerified(NetUserId userId, CancellationToken cancel = default)
    {
        _sawmill.Debug($"Player {userId} check Discord verification");

        var requestUrl = $"{_apiUrl}/api/uuid?method=uid&id={userId}";
        _sawmill.Debug($"Auth request url:{requestUrl}");
        var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        var response = await _httpClient.SendAsync(request, cancel);

        _sawmill.Debug($"{await response.Content.ReadAsStringAsync(cancel)}");
        _sawmill.Debug($"{(int)response.StatusCode}");
        var verified = response.StatusCode == HttpStatusCode.OK;
        var guildsVerified = await CheckGuilds(userId, cancel);

        if (!verified)
            return new AuthData { Verified = false, ErrorMessage = Loc.GetString("cp14-discord-info")};

        return guildsVerified;
    }

    private async Task<AuthData> CheckGuilds(NetUserId userId, CancellationToken cancel = default)
    {
        var isWhitelisted = await _db.GetWhitelistStatusAsync(userId);
        if (isWhitelisted)
        {
            _sawmill.Debug($"Player {userId} is whitelisted");
            return new AuthData { Verified = true };
        }

        _sawmill.Debug($"Checking guilds for {userId}");

        var requestUrl = $"{_apiUrl}/api/guilds?method=uid&id={userId}";
        _sawmill.Debug($"Guilds request url:{requestUrl}");
        var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        var response = await _httpClient.SendAsync(request, cancel);
        _sawmill.Debug($"Guilds response: {await response.Content.ReadAsStringAsync(cancel)}");
        _sawmill.Debug($"(int) response.StatusCode: {(int)response.StatusCode}");
        if (!response.IsSuccessStatusCode)
        {
            _sawmill.Debug($"Player {userId} guilds check failed: !response.IsSuccessStatusCode");
            return new AuthData { Verified = false, ErrorMessage = "Unexpected error: !response.IsSuccessStatusCode" };
        }

        var guilds = await response.Content.ReadFromJsonAsync<DiscordGuildsResponse>(cancel);
        if (guilds is null)
        {
            _sawmill.Debug($"Player {userId} guilds check failed: guilds is null");
            return new AuthData { Verified = false, ErrorMessage = "Unexpected error: guilds is null" };
        }

        foreach (var guild in guilds.Guilds)
        {
            if (_blockedGuilds.Contains(guild.Id))
            {
                var encodedMessage = "RXJyb3IgMjcwMQ==";
                var errorMessage = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encodedMessage));
                return new AuthData { Verified = false, ErrorMessage = errorMessage };
            }
        }

        if (guilds.Guilds.All(guild => guild.Id != _discordGuild))
        {
            _sawmill.Debug($"Player {userId} is not in required guild {_discordGuild}");
            return new AuthData { Verified = false, ErrorMessage = "You are not a member of the CrystallEdge server." };
        }

        return new AuthData { Verified = true };
    }

    public async Task<string?> GenerateLink(NetUserId userId, CancellationToken cancel = default)
    {
        _sawmill.Debug($"Generating link for {userId}");
        var requestUrl = $"{_apiUrl}/api/link?uid={userId}";

        // try catch block to catch HttpRequestExceptions due to remote service unavailability
        try
        {
            var response = await _httpClient.GetAsync(requestUrl, cancel);
            if (!response.IsSuccessStatusCode)
                return null;

            var link = await response.Content.ReadFromJsonAsync<DiscordLinkResponse>(cancel);
            return link!.Link;
        }
        catch (HttpRequestException)
        {
            _sawmill.Error("Remote auth service is unreachable. Check if its online!");
            return null;
        }
        catch (Exception e)
        {
            _sawmill.Error(
                $"Unexpected error verifying user via auth service. Error: {e.Message}. Stack: \n{e.StackTrace}");
            return null;
        }
    }

    sealed class DiscordLinkResponse
    {
        [JsonPropertyName("link")]
        public string Link { get; set; } = string.Empty;
    }

    private sealed class DiscordGuildsResponse
    {
        [JsonPropertyName("guilds")]
        public DiscordGuild[] Guilds { get; set; } = [];
    }

    private sealed class DiscordGuild
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;
    }

    public sealed class AuthData
    {
        public bool Verified { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
