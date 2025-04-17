using System.Diagnostics.CodeAnalysis;
using Lidgren.Network;
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

public sealed class CP14SponsorRoleUpdate : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.Command;

    public ProtoId<CP14SponsorRolePrototype> Role { get; set; }

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        Role = buffer.ReadString();
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.Write(Role);
    }
}
