using Content.Server.Chat.Managers;
using Content.Server.Speech;
using Content.Shared._CP14.Religion.Components;
using Content.Shared._CP14.Religion.Prototypes;
using Content.Shared._CP14.Religion.Systems;
using Content.Shared.Chat;
using Robust.Server.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Religion;

public sealed partial class CP14ReligionGodSystem : CP14SharedReligionGodSystem
{
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly PvsOverrideSystem _pvs = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14ReligionObserverComponent, ComponentInit>(OnObserverInit);
        SubscribeLocalEvent<CP14ReligionObserverComponent, AfterAutoHandleStateEvent>(OnObserverHandleState);

        SubscribeLocalEvent<CP14ReligionEntityComponent, ComponentInit>(OnGodInit);
        SubscribeLocalEvent<CP14ReligionEntityComponent, ComponentShutdown>(OnGodShutdown);
        SubscribeLocalEvent<CP14ReligionEntityComponent, PlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<CP14ReligionEntityComponent, PlayerDetachedEvent>(OnPlayerDetached);

        SubscribeLocalEvent<CP14ReligionAltarComponent, ListenEvent>(OnListen);
    }

    private void OnObserverHandleState(Entity<CP14ReligionObserverComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        var query = EntityQueryEnumerator<CP14ReligionEntityComponent>();
        while (query.MoveNext(out var uid, out var god))
        {
            UpdatePvsOverrides(new Entity<CP14ReligionEntityComponent>(uid, god));
        }
    }

    private void OnObserverInit(Entity<CP14ReligionObserverComponent> ent, ref ComponentInit args)
    {
        foreach (var (religion, _) in ent.Comp.Observation)
        {
            var gods = GetGods(religion);

            foreach (var god in gods)
            {
                UpdatePvsOverrides(god);
            }
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

        var wrappedMessage =
            Loc.GetString("cp14-altar-wrapped-message", ("name", MetaData(args.Source).EntityName), ("msg", args.Message));

        SendMessageToGods(ent.Comp.Religion.Value, wrappedMessage, args.Source);
    }

    public override void SendMessageToGods(ProtoId<CP14ReligionPrototype> religion, string msg, EntityUid source)
    {
        var gods = GetGods(religion);

        HashSet<INetChannel> channels = new();
        foreach (var god in gods)
        {
            if (!TryComp<ActorComponent>(god, out var godActor))
                continue;

            channels.Add(godActor.PlayerSession.Channel);
        }

        _chat.ChatMessageToMany(ChatChannel.Notifications, msg, msg, source, false, true, channels, colorOverride: Color.Aqua);
    }

    private void AddPvsOverrides(Entity<CP14ReligionEntityComponent> ent)
    {
        if (ent.Comp.Religion is null)
            return;

        if (!TryComp<ActorComponent>(ent, out var actor))
            return;

        ent.Comp.Session = actor.PlayerSession;

        var query = EntityQueryEnumerator<CP14ReligionObserverComponent>();
        while (query.MoveNext(out var uid, out var observer))
        {
            if (!observer.Observation.ContainsKey(ent.Comp.Religion.Value))
                continue;

            if (observer.Observation[ent.Comp.Religion.Value] <= 6.5f) //Maybe there is a variable for the distance outside the screen in PVS, I don't know. This number works best
                continue;

            ent.Comp.PvsOverridedObservers.Add(uid);
            _pvs.AddSessionOverride(uid, actor.PlayerSession);
        }
    }

    private void RemovePvsOverrides(Entity<CP14ReligionEntityComponent> ent)
    {
        if (ent.Comp.Religion is null)
            return;

        if (ent.Comp.Session is null)
            return;

        foreach (var altar in ent.Comp.PvsOverridedObservers)
        {
            _pvs.RemoveSessionOverride(altar, ent.Comp.Session);
        }

        ent.Comp.Session = null;
        ent.Comp.PvsOverridedObservers.Clear();
    }

    private void UpdatePvsOverrides(Entity<CP14ReligionEntityComponent> ent)
    {
        if (ent.Comp.Session is null)
            return;

        RemovePvsOverrides(ent);
        AddPvsOverrides(ent);
    }
}
