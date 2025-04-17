using System.Diagnostics.CodeAnalysis;
using Content.Shared._CP14.Sponsor;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Client._CP14.Sponsor;

public sealed class ClientSponsorSystem : ICP14SponsorManager
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IClientNetManager _net = default!;

    private CP14SponsorRolePrototype? _sponsorRole;

    public void Initialize()
    {
        _net.RegisterNetMessage<CP14SponsorRoleUpdate>(OnSponsorRoleUpdate);
        _net.Disconnect += NetOnDisconnected;
    }

    private void NetOnDisconnected(object? sender, NetDisconnectedArgs e)
    {
        _sponsorRole = null;
    }

    private void OnSponsorRoleUpdate(CP14SponsorRoleUpdate msg)
    {
        if (!_proto.TryIndex(msg.Role, out var indexedRole))
            return;

        _sponsorRole = indexedRole;
    }

    public bool TryGetSponsorOOCColor(NetUserId userId, [NotNullWhen(true)] out Color? color)
    {
        throw new NotImplementedException();
    }

    public bool UserHasFeature(NetUserId userId, ProtoId<CP14SponsorFeaturePrototype> feature, bool ifDisabledSponsorhip = true)
    {
        if (_sponsorRole is null)
            return false;

        if (!_proto.TryIndex(feature, out var indexedFeature))
            return false;

        return _sponsorRole.Priority >= indexedFeature.MinPriority;
    }
}
