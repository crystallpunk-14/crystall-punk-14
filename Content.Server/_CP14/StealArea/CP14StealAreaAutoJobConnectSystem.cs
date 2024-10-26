using Content.Server.GameTicking;
using Content.Server.Mind;
using Content.Server.Objectives.Components;
using Content.Shared.Roles.Jobs;
using Robust.Server.Player;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.StealArea;

public sealed class CP14StealAreaAutoJobConnectSystem : EntitySystem
{

    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly SharedJobSystem _job = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14StealAreaAutoJobConnectComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawning);
    }

    private void OnMapInit(Entity<CP14StealAreaAutoJobConnectComponent> autoConnect, ref MapInitEvent args)
    {
        if (!TryComp<StealAreaComponent>(autoConnect, out var stealArea))
            return;

        foreach (var player in _playerManager.Sessions)
        {
            if (!_mind.TryGetMind(player.UserId, out var playerMind))
                continue;

            if (!_job.MindTryGetJob(playerMind, out var playerJob))
                continue;

            if (stealArea.Owners.Contains(playerMind.Value))
                continue;

            if (autoConnect.Comp.Jobs.Contains(playerJob))
            {
                stealArea.Owners.Add(playerMind.Value);
                break;
            }

            foreach (var depObj in autoConnect.Comp.Departments)
            {
                if (!_proto.TryIndex(depObj, out var indexedDepart))
                    continue;

                if (!indexedDepart.Roles.Contains(playerJob))
                    continue;

                stealArea.Owners.Add(playerMind.Value);
                break;
            }
        }
    }

    private void OnPlayerSpawning(PlayerSpawnCompleteEvent ev)
    {
        if (!_mind.TryGetMind(ev.Player.UserId, out var mind))
            return;

        var query = EntityQueryEnumerator<StealAreaComponent, CP14StealAreaAutoJobConnectComponent>();
        while (query.MoveNext(out var uid, out var stealArea, out var autoConnect))
        {
            if (stealArea.Owners.Contains(mind.Value))
                continue;

            if (ev.JobId is null)
                continue;

            if (autoConnect.Jobs.Contains(ev.JobId))
            {
                stealArea.Owners.Add(mind.Value);
                break;
            }

            foreach (var depObj in autoConnect.Departments)
            {
                if (!_proto.TryIndex(depObj, out var indexedDepart))
                    continue;

                if (!indexedDepart.Roles.Contains(ev.JobId))
                    continue;

                stealArea.Owners.Add(mind.Value);
                break;
            }
        }
    }
}
