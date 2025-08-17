using Content.Shared._CP14.Skill.Prototypes;
using Content.Shared._CP14.Vampire.Components;
using Content.Shared.CCVar;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._CP14.Skill.Restrictions;

public sealed partial class VampireTreeLevel : CP14SkillRestriction
{
    [DataField]
    public int Level = 1;

    public override bool Check(IEntityManager entManager, EntityUid target, CP14SkillPrototype skill)
    {
        if (!entManager.TryGetComponent<CP14VampireComponent>(target, out var playerVampire))
            return false;

        if (!entManager.TryGetComponent<TransformComponent>(target, out var xform))
            return false;

        var lookup = entManager.System<EntityLookupSystem>();

        foreach (var tree in lookup.GetEntitiesInRange<CP14VampireTreeComponent>(xform.Coordinates, 2))
        {
            if (tree.Comp.Faction != playerVampire.Faction)
                continue;

            if (tree.Comp.TreeLevel < Level)
                continue;

            return true;
        }

        return false;
    }

    public override string GetDescription(IEntityManager entManager, IPrototypeManager protoManager)
    {
        return Loc.GetString("cp14-skill-req-vampire-tree-level", ("lvl", Level));
    }
}
