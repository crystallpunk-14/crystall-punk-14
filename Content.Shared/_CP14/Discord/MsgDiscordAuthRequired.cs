using Lidgren.Network;
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Discord;

public sealed class MsgDiscordAuthRequired : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.Command;
    public string AuthUrl { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        ErrorMessage = buffer.ReadString();
        AuthUrl = buffer.ReadString();
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.Write(ErrorMessage);
        buffer.Write(AuthUrl);
    }
}
