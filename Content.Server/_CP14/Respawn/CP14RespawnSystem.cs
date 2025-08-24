using Content.Server.Chat.Systems;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Events;
using Content.Shared._CP14.Respawn;
using Content.Shared.Administration.Managers;
using Content.Shared.GameTicking;
using Content.Shared.Ghost;
using Content.Shared.Preferences;
using JetBrains.Annotations;
using Robust.Shared.Audio;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Server._CP14.Respawn;

public sealed partial class CP14RespawnSystem : EntitySystem
{
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly ISharedAdminManager _admin = default!;

    /// <summary>
    /// We keep a list of all characters connected in a round to prevent the same character from re-entering the round.
    /// Of course, we only check by character name, and this is easy to circumvent by renaming the character,
    /// but the main goal here is to make the player understand that this is not the right thing to do.
    /// If a player abuses the ability to re-enter a round, the administration will take care of it.
    /// </summary>
    private Dictionary<NetUserId, List<string>> _usedCharacters = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GhostComponent, CP14RespawnAction>(OnRespawnAction);
        SubscribeLocalEvent<CP14PlayerSpawnAttemptEvent>(OnBeforePlayerSpawn);
        SubscribeLocalEvent<RoundStartingEvent>(OnRoundEnd);
    }

    private void OnRoundEnd(RoundStartingEvent ev)
    {
        _usedCharacters.Clear();
    }

    private void OnBeforePlayerSpawn(CP14PlayerSpawnAttemptEvent ev)
    {
        if (_admin.IsAdmin(ev.Player, true))
            return;

        if (!_usedCharacters.TryGetValue(ev.Player.UserId, out var usedCharacters))
        {
            usedCharacters = [];
            _usedCharacters[ev.Player.UserId] = usedCharacters;
        }

        if (usedCharacters.Contains(ev.Profile.Name))
        {
            var filter = Filter.Empty().AddPlayer(ev.Player);
            _chat.DispatchFilteredAnnouncement(
                filter,
                Loc.GetString("cp14-respawn-same-char-bloked"),
                colorOverride: Color.Red,
                announcementSound: new SoundPathSpecifier("/Audio/Effects/beep1.ogg"),
                sender: "Server");
            ev.Cancel();
            return;
        }

        usedCharacters.Add(ev.Profile.Name);
    }

    private void OnRespawnAction(Entity<GhostComponent> ent, ref CP14RespawnAction args)
    {
        if (!TryComp<ActorComponent>(ent, out var actor))
            return;

        _gameTicker.Respawn(actor.PlayerSession);
    }
}

/// <summary>
/// Called before a player spawns for a specific character. Can be canceled by prohibiting connection for that character.
/// </summary>
[PublicAPI]
public sealed class CP14PlayerSpawnAttemptEvent(
    ICommonSession player,
    HumanoidCharacterProfile profile,
    string? jobId,
    bool lateJoin,
    EntityUid station)
    : CancellableEntityEventArgs
{
    public ICommonSession Player { get; } = player;
    public HumanoidCharacterProfile Profile { get; } = profile;
    public string? JobId { get; } = jobId;
    public bool LateJoin { get; } = lateJoin;
    public EntityUid Station { get; } = station;
}
