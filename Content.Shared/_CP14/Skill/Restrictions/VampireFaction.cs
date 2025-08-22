using Content.Shared._CP14.Vampire;
using Content.Shared._CP14.Vampire.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Skill.Restrictions;

public sealed partial class VampireFaction : CP14SkillRestriction
{
    public override bool HideFromUI => true;

    [DataField(required: true)]
    public ProtoId<CP14VampireFactionPrototype> Clan;

    public override bool Check(IEntityManager entManager, EntityUid target)
    {
        if (!entManager.TryGetComponent<CP14VampireComponent>(target, out var vampire))
            return false;

        return vampire.Faction == Clan;
    }

    public override string GetDescription(IEntityManager entManager, IPrototypeManager protoManager)
    {
        var clan = protoManager.Index(Clan);

        return Loc.GetString("cp14-skill-req-vampire-clan", ("name", Loc.GetString(clan.Name)));
    }
}
