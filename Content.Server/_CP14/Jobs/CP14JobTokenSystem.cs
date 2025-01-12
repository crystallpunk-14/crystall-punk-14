using Content.Shared.GameTicking;
using Content.Shared.Inventory;
using Content.Shared.Mind;
using Robust.Server.Player;

namespace Content.Server._CP14.Jobs;

public partial class CP14JobTokenSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(GivePlayerToken);
    }

    private void GivePlayerToken(PlayerSpawnCompleteEvent ev)
    {
        var playerName = ev.Player.Name;
        var playerEntity = ev.Player.AttachedEntity;

        if (playerEntity is null)
            return;

        if (!_inventory.TryGetSlot(playerEntity.Value, "back", out var backSlot))
            return;

        var token = Spawn("CP14Paper");

    }
}
