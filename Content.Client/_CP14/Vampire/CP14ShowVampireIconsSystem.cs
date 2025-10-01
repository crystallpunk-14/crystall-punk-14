using Content.Client.Administration.Managers;
using Content.Client.Overlays;
using Content.Shared._CP14.Vampire.Components;
using Content.Shared.Ghost;
using Content.Shared.StatusIcon.Components;
using Robust.Client.Player;
using Robust.Shared.Prototypes;

namespace Content.Client._CP14.Vampire;

public sealed class CP14ShowVampireIconsSystem : EquipmentHudSystem<CP14ShowVampireFactionComponent>
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IClientAdminManager _admin = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14VampireComponent, GetStatusIconsEvent>(OnGetStatusIcons);
    }

    private void OnGetStatusIcons(Entity<CP14VampireComponent> ent, ref GetStatusIconsEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.Faction, out var indexedFaction))
            return;

        if (!IsActive || !_proto.TryIndex(indexedFaction.FactionIcon, out var indexedIcon))
            return;

        // Show icons for admins
        if (_admin.IsAdmin() && HasComp<GhostComponent>(_player.LocalEntity))
        {
            args.StatusIcons.Add(indexedIcon);
            return;
        }

        if (TryComp<CP14ShowVampireFactionComponent>(_player.LocalEntity, out var showIcons) &&
            showIcons.Faction == indexedFaction)
        {
            args.StatusIcons.Add(indexedIcon);
            return;
        }
    }
}
