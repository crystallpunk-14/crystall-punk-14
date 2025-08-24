using Content.Server.GameTicking;
using Content.Shared._CP14.Respawn;
using Content.Shared.Ghost;
using Robust.Shared.Player;

namespace Content.Server._CP14.Respawn;

public sealed partial class CP14RespawnSystem : EntitySystem
{
    [Dependency] private readonly GameTicker _gameTicker = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GhostComponent, CP14RespawnAction>(OnRespawnAction);
    }

    private void OnRespawnAction(Entity<GhostComponent> ent, ref CP14RespawnAction args)
    {
        if (!TryComp<ActorComponent>(ent, out var actor))
            return;

        _gameTicker.Respawn(actor.PlayerSession);
    }
}
