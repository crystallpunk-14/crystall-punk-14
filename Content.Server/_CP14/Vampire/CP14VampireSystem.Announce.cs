using System.Text;
using Content.Server.Chat.Systems;
using Content.Shared._CP14.Vampire;
using Content.Shared._CP14.Vampire.Components;
using Content.Shared.Examine;
using Content.Shared.Ghost;
using Robust.Shared.Audio;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Vampire;

public sealed partial class CP14VampireSystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    private void InitializeAnnounces()
    {
        SubscribeLocalEvent<CP14VampireClanHeartComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(Entity<CP14VampireClanHeartComponent> ent, ref ExaminedEvent args)
    {
        if (!HasComp<CP14VampireComponent>(args.Examiner) && !HasComp<GhostComponent>(args.Examiner))
            return;

        var sb = new StringBuilder();

        // Faction
        if (Proto.TryIndex(ent.Comp.Faction, out var indexedFaction))
            sb.Append(Loc.GetString("cp14-vampire-tree-examine-faction", ("faction", Loc.GetString(indexedFaction.Name))) + "\n");

        // Are they friend or foe?
        if (TryComp<CP14VampireComponent>(args.Examiner, out var examinerVampire))
        {
            if (examinerVampire.Faction == ent.Comp.Faction)
                sb.Append(Loc.GetString("cp14-vampire-tree-examine-friend") + "\n");
            else
                sb.Append(Loc.GetString("cp14-vampire-tree-examine-enemy") + "\n");
        }

        //Progress
        sb.Append(Loc.GetString("cp14-vampire-tree-examine-level",
            ("level", ent.Comp.Level),
            ("essence", ent.Comp.EssenceFromLevelStart),
            ("left", ent.Comp.EssenceToNextLevel?.ToString() ?? "∞")) + "\n"+ "\n");

        var query = EntityQueryEnumerator<CP14VampireClanHeartComponent>();

        sb.Append(Loc.GetString("cp14-vampire-tree-other-title") + "\n");
        while (query.MoveNext(out var uid, out var heart))
        {
            if (uid == ent.Owner)
                continue;

            if (!Proto.TryIndex(heart.Faction, out var indexedOtherFaction))
                continue;

            sb.Append(Loc.GetString("cp14-vampire-tree-other-info",
                ("name", Loc.GetString(indexedOtherFaction.Name)),
                ("essence", heart.EssenceFromLevelStart),
                ("left", heart.EssenceToNextLevel?.ToString() ?? "∞"),
                ("lvl", heart.Level)) + "\n");
        }

        args.PushMarkup(sb.ToString());
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
