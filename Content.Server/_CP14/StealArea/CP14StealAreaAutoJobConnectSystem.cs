using Content.Server.GameTicking;
using Content.Server.Mind;
using Content.Server.Objectives.Components;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.StealArea;

public sealed class CP14StealAreaAutoJobConnectSystem : EntitySystem
{

    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawning);
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
