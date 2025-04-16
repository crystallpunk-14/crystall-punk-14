using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Content.Server._CP14.Discord;
using Content.Shared._CP14.Sponsor;
using Content.Shared.CCVar;
using Robust.Shared.Configuration;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Sponsor;

public sealed class SponsorManager
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly DiscordAuthManager _discordAuthManager = default!;
    [Dependency] private readonly INetManager _netMgr = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    private readonly HttpClient _httpClient = new();
    private string _apiUrl = string.Empty;
    private string _apiKey = string.Empty;
    private bool _enabled;

    private ISawmill _sawmill = null!;

    private Dictionary<NetUserId, HashSet<CP14SponsorRolePrototype>> _cachedSponsors = new();

    public void Initialize()
    {
        _sawmill = Logger.GetSawmill("sponsors");

        _cfg.OnValueChanged(CCVars.DiscordAuthEnabled, val => { _enabled = val; }, true);
        _cfg.OnValueChanged(CCVars.DiscordAuthUrl, val => { _apiUrl = val; }, true);
        _cfg.OnValueChanged(CCVars.DiscordAuthToken, val => { _apiKey = val; }, true);

        _discordAuthManager.PlayerVerified += OnPlayerVerified;
        _netMgr.Disconnect += OnDisconnect;

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _apiKey);
    }

    private async Task<List<string>?> GetRoles(NetUserId userId)
    {
        var requestUrl = $"{_apiUrl}/roles?method=uid&id={userId}&guildId={DiscordAuthManager.DISCORD_GUILD}";
        var response = await _httpClient.GetAsync(requestUrl);

        if (!response.IsSuccessStatusCode)
        {
            _sawmill.Error($"Failed to retrieve roles for user {userId}: {response.StatusCode}");
            return null;
        }

        var responseContent = await response.Content.ReadFromJsonAsync<RolesResponse>();

        if (responseContent is not null)
        {
            return responseContent.Roles.ToList();
        }

        _sawmill.Error($"Roles not found in response for user {userId}");
        return null;
    }

    private async void OnPlayerVerified(object? sender, ICommonSession e)
    {
        if (!_enabled)
            return;

        var roles = await GetRoles(e.UserId);
        if (roles is null)
            return;

        foreach (var role in _prototype.EnumeratePrototypes<CP14SponsorRolePrototype>())
        {
            if (!roles.Contains(role.DiscordRoleId))
                continue;

            if (!_cachedSponsors.TryGetValue(e.UserId, out var value))
            {
                value = new HashSet<CP14SponsorRolePrototype>();
                _cachedSponsors[e.UserId] = value;
            }

            value.Add(role);
        }
    }

    private void OnDisconnect(object? sender, NetDisconnectedArgs e)
    {
        //Remove cached roles
        if (_cachedSponsors.ContainsKey(e.Channel.UserId))
        {
            _cachedSponsors.Remove(e.Channel.UserId);
        }
    }

    public bool UserHasFeature(NetUserId userId, ProtoId<CP14SponsorFeaturePrototype> feature, bool ifDisabledSponsorhip = true)
    {
        if (!_enabled)
            return ifDisabledSponsorhip;

        if (!_prototype.TryIndex(feature, out var indexedFeature))
            return false;

        if (!_cachedSponsors.TryGetValue(userId, out var userRoles))
            return false;

        foreach (var role in userRoles)
        {
            if (indexedFeature.ForRoles.Contains(role))
                return true;
        }

        return false;
    }

    public bool TryGetSponsorOOCColor(NetUserId userId, [NotNullWhen(true)] out Color? color)
    {
        color = null;

        if (!_enabled)
            return false;

        if (!_cachedSponsors.TryGetValue(userId, out var sponsorRoles))
            return false;

        var priority = 0;
        foreach (var role in sponsorRoles)
        {
            if (role.ColorOOC is not null && role.ColorPriority > priority)
                color = role.ColorOOC;
        }

        return color is not null;
    }

    private sealed class RolesResponse
    {
        [JsonPropertyName("roles")]
        public string[] Roles { get; set; } = [];
    }
}
