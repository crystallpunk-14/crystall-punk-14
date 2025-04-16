using System.Diagnostics.CodeAnalysis;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Sponsor;

public interface ICP14SponsorManager
{
    public void Initialize();

    public bool UserHasFeature(NetUserId userId,
        ProtoId<CP14SponsorFeaturePrototype> feature,
        bool ifDisabledSponsorhip = true);

    public bool TryGetSponsorOOCColor(NetUserId userId, [NotNullWhen(true)] out Color? color);
}

[Serializable, NetSerializable]
public sealed class CP14SponsorRoleEvent : EntityEventArgs
{
    public ProtoId<CP14SponsorRolePrototype> Role = new();
    public CP14SponsorRoleEvent(ProtoId<CP14SponsorRolePrototype> role)
    {
        Role = role;
    }
}
