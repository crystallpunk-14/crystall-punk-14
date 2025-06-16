using Content.Shared._CP14.Skill;

namespace Content.Shared._CP14.MagicSpell.Spells;

public sealed partial class CP14SpellRemoveMemoryPoint : CP14SpellEffect
{
    [DataField]
    public float RemovedPoints = 0.5f;

    public override void Effect(EntityManager entManager, CP14SpellEffectBaseArgs args)
    {
        if (args.Target is null)
            return;

        var skillSys = entManager.System<CP14SharedSkillSystem>();

        skillSys.RemoveMemoryPoints(args.Target.Value, RemovedPoints);
    }
}
