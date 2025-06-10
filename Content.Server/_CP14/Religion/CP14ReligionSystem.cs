using Content.Server.Chat.Managers;
using Content.Server.Speech;
using Content.Shared._CP14.Religion.Components;
using Content.Shared._CP14.Religion.Systems;
using Content.Shared.Chat;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Server._CP14.Religion;

public sealed partial class CP14ReligionGodSystem : CP14SharedReligionGodSystem
{
    [Dependency] private readonly IChatManager _chat = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14ReligionAltarComponent, ListenEvent>(OnListen);
    }

    private void OnListen(Entity<CP14ReligionAltarComponent> ent, ref ListenEvent args)
    {
        if (ent.Comp.Religion is null)
            return;

        var gods = GetGods(ent.Comp.Religion.Value);
        var wrappedMessage =
            Loc.GetString("cp14-altar-wrapped-message", ("name", MetaData(args.Source).EntityName), ("msg", args.Message));

        HashSet<INetChannel> channels = new();
        foreach (var god in gods)
        {
            if (!TryComp<ActorComponent>(god, out var godActor))
                continue;

            channels.Add(godActor.PlayerSession.Channel);
        }

        _chat.ChatMessageToMany(ChatChannel.Notifications, args.Message, wrappedMessage, args.Source, false, true, channels);
    }
}
