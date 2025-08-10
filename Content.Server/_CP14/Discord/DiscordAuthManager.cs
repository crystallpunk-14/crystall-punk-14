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
    // Suspicious activity blocking stuff
    private string _suspiciousAccountsWarningLevel = string.Empty;
    private bool _panicBunkerEnabled;
    private string _panicBunkerCustomReason = string.Empty;
    private bool _panicBunkerShowReason;

    public const string RequiredDiscordGuild = "1221923073759121468"; //CrystallEdge server required

    private HashSet<string> _blockedGuilds = new()
    {
        "1346922008000204891",
        "1186566619858731038",
        "1355279097906855968",
        "1352009516941705216",
        "1359476387190145034",
        "1294276016117911594",
        "1278755078315970620",
        "1330772249644630157",
        "1274951101464051846",
    };

    public event EventHandler<ICommonSession>? PlayerVerified;

    public void Initialize()
    {
        _sawmill = Logger.GetSawmill("discordAuth");

        _cfg.OnValueChanged(CCVars.DiscordAuthEnabled, v => _enabled = v, true);
        _cfg.OnValueChanged(CCVars.DiscordAuthUrl, v => _apiUrl = v, true);
        _cfg.OnValueChanged(CCVars.DiscordAuthToken, v => _apiKey = v, true);
        // Suspicious activity blocking stuff
        _cfg.OnValueChanged(CCVars.SuspiciousAccountsWarningLevel, v => _suspiciousAccountsWarningLevel = v, true);
        _cfg.OnValueChanged(CCVars.PanicBunkerEnabled, v => _panicBunkerEnabled = v, true);
        _cfg.OnValueChanged(CCVars.PanicBunkerCustomReason, v => _panicBunkerCustomReason = v, true);
        _cfg.OnValueChanged(CCVars.PanicBunkerShowReason, v => _panicBunkerShowReason = v, true);

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

        if (!verified)
            return new AuthData { Verified = false, ErrorMessage = Loc.GetString("cp14-discord-info")};

        var userVerified = await VerifyUser(userId, cancel);
        return userVerified;
    }

    private async Task<AuthData> VerifyUser(NetUserId userId, CancellationToken cancel = default)
    {
        var isSuspicious = false;
        var isWhitelisted = await _db.GetWhitelistStatusAsync(userId);
        if (isWhitelisted)
        {
            return new AuthData { Verified = true };
        }

        var guilds = await GetUserGuilds(userId, cancel);
        if (guilds.Guilds is [])
        {
            return new AuthData { Verified = false, ErrorMessage = guilds.ErrorMessage };
        }

        foreach (var guild in guilds.Guilds)
        {
            if (_blockedGuilds.Contains(guild.Id))
            {
                isSuspicious = true;
                break;
            }
        }

        if (guilds.Guilds.All(guild => guild.Id != RequiredDiscordGuild))
        {
            _sawmill.Debug($"Player {userId} is not in required guild {RequiredDiscordGuild}");
            return new AuthData { Verified = false, ErrorMessage = "You are not a member of the CrystallEdge server." };
        }

        var user = await GetDiscordUser(userId, cancel);
        if (user.Id == string.Empty)
        {
            return new AuthData { Verified = false, ErrorMessage = user.ErrorMessage };
        }

        var accountAge = GetAccountAge(user.Id);
        if (accountAge < 45)
        {
            isSuspicious = true;
        }

        // Fastest way to block user is just not verify it
        switch (_suspiciousAccountsWarningLevel)
        {
            case "medium":
            {
                if (_panicBunkerEnabled)
                {
                    var errorMessage =
                        System.Text.Encoding.UTF8.GetString(Convert.FromBase64String("RXJyb3IgMjcwMQ=="));
                    if (_panicBunkerShowReason)
                    {
                        errorMessage = "Panic bunker enabled";
                        if (_panicBunkerCustomReason != string.Empty)
                        {
                            errorMessage = _panicBunkerCustomReason;
                        }
                    }

                    return new AuthData { Verified = false, ErrorMessage = errorMessage };
                }

                break;
            }
            case "high":
            {
                return new AuthData
                {
                    Verified = false,
                    ErrorMessage = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String("RXJyb3IgMjcwMQ=="))
                };
            }
        }

        return new AuthData { Verified = true, Suspicious = isSuspicious };
    }

    private async Task<DiscordGuildsResponse> GetUserGuilds(NetUserId userId, CancellationToken cancel = default)
    {
        _sawmill.Debug($"Checking guilds for {userId}");

        var requestUrl = $"{_apiUrl}/api/guilds?method=uid&id={userId}";
        _sawmill.Debug($"Guilds request url:{requestUrl}");

        var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        var response = await _httpClient.SendAsync(request, cancel);
        _sawmill.Debug($"(int) response.StatusCode: {(int)response.StatusCode}");
        if (!response.IsSuccessStatusCode)
        {
            _sawmill.Debug($"Player {userId} guilds check failed: {!response.IsSuccessStatusCode}");
            return new DiscordGuildsResponse { ErrorMessage = $"Unexpected error: {!response.IsSuccessStatusCode}" };
        }

        var guilds = await response.Content.ReadFromJsonAsync<DiscordGuildsResponse>(cancel);
        if (guilds is null)
        {
            _sawmill.Debug($"Player {userId} guilds check failed: guilds is null");
            return new DiscordGuildsResponse { ErrorMessage = "Unexpected error: guilds is null" };
        }
        _sawmill.Debug($"Player {userId} guilds check succeed.");
        return new  DiscordGuildsResponse { Guilds = guilds.Guilds };
    }

    private async Task<DiscordUserResponse> GetDiscordUser(NetUserId userId, CancellationToken cancel = default)
    {
        _sawmill.Debug($"Checking account age for {userId}");

        var requestUrl = $"{_apiUrl}/api/users/@me";
        _sawmill.Debug($"User request url:{requestUrl}");

        var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        var response = await _httpClient.SendAsync(request, cancel);
        _sawmill.Debug($"(int) response.StatusCode: {(int)response.StatusCode}");
        if (!response.IsSuccessStatusCode)
        {
            _sawmill.Debug($"Player {userId} user age check failed: {!response.IsSuccessStatusCode}");
            return new DiscordUserResponse { ErrorMessage = $"Unexpected error: {!response.IsSuccessStatusCode}" };
        }

        var user = await response.Content.ReadFromJsonAsync<DiscordUserResponse>(cancel);
        if (user is null)
        {
            _sawmill.Debug($"Player {userId} user age check failed: user is null");
            return new DiscordUserResponse { ErrorMessage = "Unexpected error: user is null" };
        }
        _sawmill.Debug($"Player {userId} user id get succeed.");
        return new DiscordUserResponse { Id = user.Id };
    }

    private double GetAccountAge(string id)
    {
        // Please check https://discord.com/developers/docs/reference#convert-snowflake-to-datetime
        var intId = Convert.ToInt32(id);
        var snowflakeCreationDateBin = Convert.ToString(intId, 2).Substring(42);
        var snowflakeCreationDateDecimal = Convert.ToInt32(snowflakeCreationDateBin) + 1420070400000;
        var accountCreationDate = DateTime.UnixEpoch.AddSeconds(snowflakeCreationDateDecimal);
        var accountAge = DateTime.Now.Subtract(accountCreationDate);
        return accountAge.TotalDays;
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
        public string ErrorMessage { get; set; } = string.Empty;
    }

    private sealed class DiscordUserResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
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
        public bool Suspicious { get; set; }
    }
}
