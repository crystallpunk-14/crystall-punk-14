using Content.Shared._CP14.Skill;
using Content.Shared._CP14.Skill.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicSpell.Spells;

public sealed partial class CP14SpellRemoveMemoryPoint : CP14SpellEffect
{
    [DataField]
    public float RemovedPoints = 0.5f;

    [DataField]
    public ProtoId<CP14SkillPointPrototype> SkillPointType = "Memory";

    public override void Effect(EntityManager entManager, CP14SpellEffectBaseArgs args)
    {
        if (args.Target is null)
            return;

        var skillSys = entManager.System<CP14SharedSkillSystem>();

        skillSys.RemoveMemoryPoints(args.Target.Value, SkillPointType, RemovedPoints);
    }
}
