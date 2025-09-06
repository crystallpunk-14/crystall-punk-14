using Content.Server.Administration.Managers;
using Content.Server.Chat.Systems;
using Content.Shared._CP14.Vampire;
using Content.Shared._CP14.Vampire.Components;
using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Vampire;

public sealed partial class CP14VampireSystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly IAdminManager _admin = default!;

    private void InitializeAnnounces()
    {
        SubscribeLocalEvent<CP14VampireClanHeartComponent, DamageChangedEvent>(OnHeartDamaged);
        SubscribeLocalEvent<CP14VampireClanHeartComponent, ComponentRemove>(OnHeartDestructed);
    }


    private void OnHeartDamaged(Entity<CP14VampireClanHeartComponent> ent, ref DamageChangedEvent args)
    {
        if (ent.Comp.Faction is null)
            return;

        if (!args.DamageIncreased)
            return;

        if (_timing.CurTime < ent.Comp.NextAnnounceTime)
            return;

        ent.Comp.NextAnnounceTime = _timing.CurTime + ent.Comp.MaxAnnounceFreq;

        AnnounceToFaction(ent.Comp.Faction.Value, Loc.GetString("cp14-vampire-tree-damaged"));
    }

    private void OnHeartDestructed(Entity<CP14VampireClanHeartComponent> ent, ref ComponentRemove args)
    {
        if (ent.Comp.Faction is null)
            return;

        if (!Proto.TryIndex(ent.Comp.Faction, out var indexedFaction))
            return;

        AnnounceToFaction(ent.Comp.Faction.Value, Loc.GetString("cp14-vampire-tree-destroyed-self"));
        AnnounceToOpposingFactions(ent.Comp.Faction.Value, Loc.GetString("cp14-vampire-tree-destroyed", ("name", Loc.GetString(indexedFaction.Name))));
    }

    public void AnnounceToFaction(ProtoId<CP14VampireFactionPrototype> faction, string message)
    {
        var filter = Filter.Empty();
        var query = EntityQueryEnumerator<CP14VampireComponent, ActorComponent>();

        while (query.MoveNext(out var uid, out var vampire, out var actor))
        {
            if (vampire.Faction != faction)
                continue;

            filter.AddPlayer(actor.PlayerSession);
        }

        if (filter.Count == 0)
            return;

        VampireAnnounce(filter, message);
    }

    public void AnnounceToOpposingFactions(ProtoId<CP14VampireFactionPrototype> faction, string message)
    {
        var filter = Filter.Empty();
        var query = EntityQueryEnumerator<CP14VampireComponent, ActorComponent>();

        while (query.MoveNext(out var uid, out var vampire, out var actor))
        {
            if (vampire.Faction == faction)
                continue;

            filter.AddPlayer(actor.PlayerSession);
        }

        foreach (var adminSession in _admin.ActiveAdmins)
        {
            filter.AddPlayer(adminSession);
        }

        if (filter.Count == 0)
            return;

        VampireAnnounce(filter, message);
    }

    private void VampireAnnounce(Filter players, string message)
    {
        _chat.DispatchFilteredAnnouncement(
            players,
            message,
            sender: Loc.GetString("cp14-vampire-sender"),
            announcementSound: new SoundPathSpecifier("/Audio/_CP14/Announce/vampire.ogg"),
            colorOverride: Color.FromHex("#820e22"));
    }
}
