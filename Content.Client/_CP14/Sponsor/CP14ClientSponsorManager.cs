using Content.Shared._CP14.Sponsor;
using Robust.Client.Player;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Client._CP14.Sponsor;

public sealed class ClientSponsorSystem : SharedSponsorSystem
{
    [Dependency] private readonly IPlayerManager _player = default!;

    private HashSet<CP14SponsorRolePrototype> _sponsorRoles = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<CP14SponsorRolesEvent>(OnGetSponsorRoles);
    }

    private void OnGetSponsorRoles(CP14SponsorRolesEvent ev)
    {
        _sponsorRoles.Clear();
        foreach (var role in ev.Roles)
        {
            if (!Proto.TryIndex(role, out var indexedRole))
                continue;

            _sponsorRoles.Add(indexedRole);
        }
    }

    public override bool UserHasFeature(NetUserId userId, ProtoId<CP14SponsorFeaturePrototype> feature, bool ifDisabledSponsorhip = true)
    {
        if (_player.LocalUser is null)
            return false;

        //Client dont know about other players
        if (_player.LocalUser != userId)
            return false;

        if (!Proto.TryIndex(feature, out var indexedFeature))
            return false;

        foreach (var role in _sponsorRoles)
        {
            if (role.Priority >= indexedFeature.MinPriority)
                return true;
        }

        return false;
    }
}
