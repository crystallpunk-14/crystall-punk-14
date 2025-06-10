using Content.Server.Chat.Managers;
using Content.Server.Speech;
using Content.Shared._CP14.Religion.Components;
using Content.Shared._CP14.Religion.Systems;
using Content.Shared.Chat;
using Robust.Server.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Server._CP14.Religion;

public sealed partial class CP14ReligionGodSystem : CP14SharedReligionGodSystem
{
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly PvsOverrideSystem _pvs = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14ReligionAltarComponent, ComponentInit>(OnAltarInit);
        SubscribeLocalEvent<CP14ReligionAltarComponent, CP14ReligionChangedEvent>(OnAltarReligionChanged);

        SubscribeLocalEvent<CP14ReligionEntityComponent, ComponentInit>(OnGodInit);
        SubscribeLocalEvent<CP14ReligionEntityComponent, ComponentShutdown>(OnGodShutdown);
        SubscribeLocalEvent<CP14ReligionEntityComponent, PlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<CP14ReligionEntityComponent, PlayerDetachedEvent>(OnPlayerDetached);

        SubscribeLocalEvent<CP14ReligionAltarComponent, ListenEvent>(OnListen);
    }

    private void OnAltarReligionChanged(Entity<CP14ReligionAltarComponent> ent, ref CP14ReligionChangedEvent args)
    {
        if (args.OldReligion is not null)
        {
            var oldGods = GetGods(args.OldReligion.Value);
            foreach (var god in oldGods)
            {
                UpdatePvsOverrides(god);
            }
        }
        if (args.NewReligion is not null)
        {
            var newGods = GetGods(args.NewReligion.Value);
            foreach (var god in newGods)
            {
                UpdatePvsOverrides(god);
            }
        }
    }

    private void OnAltarInit(Entity<CP14ReligionAltarComponent> ent, ref ComponentInit args)
    {
        if (ent.Comp.Religion is null)
            return;

        var gods = GetGods(ent.Comp.Religion.Value);

        foreach (var god in gods)
        {
            UpdatePvsOverrides(god);
        }
    }

    private void OnGodInit(Entity<CP14ReligionEntityComponent> ent, ref ComponentInit args)
    {
        AddPvsOverrides(ent);
    }

    private void OnGodShutdown(Entity<CP14ReligionEntityComponent> ent, ref ComponentShutdown args)
    {
        RemovePvsOverrides(ent);
    }

    private void OnPlayerAttached(Entity<CP14ReligionEntityComponent> ent, ref PlayerAttachedEvent args)
    {
        AddPvsOverrides(ent);
    }

    private void OnPlayerDetached(Entity<CP14ReligionEntityComponent> ent, ref PlayerDetachedEvent args)
    {
        RemovePvsOverrides(ent);
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

    private void AddPvsOverrides(Entity<CP14ReligionEntityComponent> ent)
    {
        if (ent.Comp.Religion is null)
            return;

        if (!TryComp<ActorComponent>(ent, out var actor))
            return;

        ent.Comp.Session = actor.PlayerSession;

        var query = EntityQueryEnumerator<CP14ReligionAltarComponent>();
        while (query.MoveNext(out var uid, out var altar))
        {
            if (altar.Religion != ent.Comp.Religion)
                continue;

            ent.Comp.PvsOverridedAltars.Add(uid);
            _pvs.AddSessionOverride(uid, actor.PlayerSession);
        }
    }

    private void RemovePvsOverrides(Entity<CP14ReligionEntityComponent> ent)
    {
        if (ent.Comp.Religion is null)
            return;

        if (ent.Comp.Session is null)
            return;

        foreach (var altar in ent.Comp.PvsOverridedAltars)
        {
            _pvs.RemoveSessionOverride(altar, ent.Comp.Session);
        }

        ent.Comp.Session = null;
        ent.Comp.PvsOverridedAltars.Clear();
    }

    private void UpdatePvsOverrides(Entity<CP14ReligionEntityComponent> ent)
    {
        if (ent.Comp.Session is null)
            return;

        RemovePvsOverrides(ent);
        AddPvsOverrides(ent);
    }
}
