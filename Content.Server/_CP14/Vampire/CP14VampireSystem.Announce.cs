using Content.Server.Chat.Systems;
using Content.Shared._CP14.Vampire;
using Content.Shared._CP14.Vampire.Components;
using Robust.Shared.Audio;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Vampire;

public sealed partial class CP14VampireSystem
{
    [Dependency] private readonly ChatSystem _chat = default!;

    private void InitializeAnnounces()
    {
        SubscribeLocalEvent<CP14VampireTreeComponent, MapInitEvent>(OnVampireTreeInit);
    }

    private void OnVampireTreeInit(Entity<CP14VampireTreeComponent> ent, ref MapInitEvent args)
    {
        if (ent.Comp.Faction is null || ent.Comp.TreeLevel is null)
            return;

        if (!Proto.TryIndex(ent.Comp.Faction, out var indexedFaction))
            return;

        AnnounceToOpposingFactions(ent.Comp.Faction.Value, Loc.GetString("cp14-vampire-tree-growing", ("name", Loc.GetString(indexedFaction.Name)), ("level", ent.Comp.TreeLevel)));
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

        _chat.DispatchFilteredAnnouncement(filter, message);
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

        VampireAnnounce(filter, message);
    }

    private void VampireAnnounce(Filter players, string message)
    {
        _chat.DispatchFilteredAnnouncement(
            players,
            message,
            announcementSound: new SoundPathSpecifier("/Audio/_CP14/Announce/vampire.ogg"),
            colorOverride: Color.FromHex("#820e22"));
    }
}
