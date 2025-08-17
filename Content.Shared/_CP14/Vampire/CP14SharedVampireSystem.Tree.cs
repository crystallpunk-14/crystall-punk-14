using Content.Shared._CP14.Vampire.Components;
using Content.Shared.Examine;

namespace Content.Shared._CP14.Vampire;

public abstract partial class CP14SharedVampireSystem
{
    private void InitializeTree()
    {
        SubscribeLocalEvent<CP14VampireTreeComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(Entity<CP14VampireTreeComponent> ent, ref ExaminedEvent args)
    {
        if (!HasComp<CP14VampireComponent>(args.Examiner))
            return;

        if (ent.Comp.TreeLevel is not null)
            args.PushMarkup(Loc.GetString("cp14-vampire-tree-examine-level", ("level", ent.Comp.TreeLevel.Value)));

        if (_proto.TryIndex(ent.Comp.Faction, out var indexedFaction))
            args.PushMarkup(Loc.GetString("cp14-vampire-tree-examine-faction", ("faction", Loc.GetString(indexedFaction.Name))));

        if (ent.Comp.NextLevelProto is not null)
            args.PushMarkup(Loc.GetString("cp14-vampire-tree-examine-essence-left", ("left", ent.Comp.EssenceToNextLevel - ent.Comp.CollectedEssence)));
    }
}
