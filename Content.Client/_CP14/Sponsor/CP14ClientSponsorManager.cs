using System.Diagnostics.CodeAnalysis;
using Content.Shared._CP14.Sponsor;
using Robust.Client.GameObjects;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Client._CP14.Sponsor;

public sealed class ClientSponsorSystem : ICP14SponsorManager, IEntityEventSubscriber
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IClientEntityManager _entityManager = default!;

    private CP14SponsorRolePrototype _sponsorRole = new();

    public void Initialize()
    {
        _entityManager.EventBus.SubscribeSessionEvent<CP14SponsorRoleEvent>(EventSource.Network, this, OnGetSponsorRoles);
    }

    private void OnGetSponsorRoles(CP14SponsorRoleEvent ev, EntitySessionEventArgs args)
    {
        if (!_proto.TryIndex(ev.Role, out var indexedRole))
            return;

        _sponsorRole = indexedRole;
    }

    public bool TryGetSponsorOOCColor(NetUserId userId, [NotNullWhen(true)] out Color? color)
    {
        throw new NotImplementedException();
    }

    public bool UserHasFeature(NetUserId userId, ProtoId<CP14SponsorFeaturePrototype> feature, bool ifDisabledSponsorhip = true)
    {
        if (!_proto.TryIndex(feature, out var indexedFeature))
            return false;

        return _sponsorRole.Priority >= indexedFeature.MinPriority;
    }
}
