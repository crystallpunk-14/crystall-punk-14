using Content.Server._CP14.MagicEnergy;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Server.Speech;
using Content.Shared._CP14.MagicEnergy.Components;
using Content.Shared._CP14.Religion.Components;
using Content.Shared._CP14.Religion.Prototypes;
using Content.Shared._CP14.Religion.Systems;
using Content.Shared.Chat;
using Content.Shared.FixedPoint;
using Robust.Server.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server._CP14.Religion;

public sealed partial class CP14ReligionGodSystem : CP14SharedReligionGodSystem
{
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly ChatSystem _chatSys = default!;
    [Dependency] private readonly PvsOverrideSystem _pvs = default!;
    [Dependency] private readonly CP14MagicEnergySystem _magicEnergy = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    public override void Initialize()
    {
        base.Initialize();
        InitializeUI();

        SubscribeLocalEvent<CP14ReligionObserverComponent, ComponentInit>(OnObserverInit);
        SubscribeLocalEvent<CP14ReligionObserverComponent, AfterAutoHandleStateEvent>(OnObserverHandleState);

        SubscribeLocalEvent<CP14ReligionEntityComponent, ComponentInit>(OnGodInit);
        SubscribeLocalEvent<CP14ReligionEntityComponent, ComponentShutdown>(OnGodShutdown);
        SubscribeLocalEvent<CP14ReligionEntityComponent, PlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<CP14ReligionEntityComponent, PlayerDetachedEvent>(OnPlayerDetached);
        SubscribeLocalEvent<CP14ReligionSpeakerComponent, CP14SpokeAttemptEvent>(OnSpokeAttempt);

        SubscribeLocalEvent<CP14ReligionAltarComponent, ListenEvent>(OnListen);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CP14ReligionFollowerComponent, CP14MagicEnergyContainerComponent>();
        while (query.MoveNext(out var uid, out var follower, out var energy))
        {
            if (follower.NextUpdateTime >= _gameTiming.CurTime)
                continue;

            if (follower.Religion is null)
                continue;

            follower.NextUpdateTime = _gameTiming.CurTime + TimeSpan.FromSeconds(follower.ManaTransferDelay);

            foreach (var god in GetGods(follower.Religion.Value))
            {
                _magicEnergy.TransferEnergy((uid, energy), god.Owner, follower.EnergyToGodTransfer, out _, out _, safe: true);
            }
        }
    }

    private void OnSpokeAttempt(Entity<CP14ReligionSpeakerComponent> ent, ref CP14SpokeAttemptEvent args)
    {
        args.Cancel();

        if (!TryComp<CP14ReligionEntityComponent>(ent, out var god) || god.Religion is null)
            return;

        if (ent.Comp.RestrictedReligionZone && !InVision(ent, (ent, god)))
            return;

        _magicEnergy.ChangeEnergy(ent.Owner, -ent.Comp.ManaCost, out _, out _);

        var speaker = Spawn(ent.Comp.Speaker);
        _transform.DropNextTo(speaker, ent.Owner);

        var message = args.Message;
        var type = args.Type;
        Timer.Spawn(333,
            () =>
            {
                _chatSys.TrySendInGameICMessage(speaker, message, type, ChatTransmitRange.Normal, nameOverride: MetaData(ent).EntityName, ignoreActionBlocker: true);
            });
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

    public FixedPoint2 GetFollowerPercentage(Entity<CP14ReligionEntityComponent> god)
    {
        var total = 0;
        var followers = 0;

        var allHumans = Mind.GetAliveHumans();
        foreach (var human in allHumans)
        {
            total++;

            if (!TryComp<CP14ReligionFollowerComponent>(human.Comp.CurrentEntity, out var relFollower))
                continue;
            if (relFollower.Religion != god.Comp.Religion)
                continue;

            followers++;
        }

        if (total == 0)
            return 0f;

        return followers / total;
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

public sealed class CP14SpokeAttemptEvent(string message, InGameICChatType type, ICommonSession? player) : CancellableEntityEventArgs
{
    public string Message = message;
    public InGameICChatType Type = type;
    public ICommonSession? Player = player;
}

