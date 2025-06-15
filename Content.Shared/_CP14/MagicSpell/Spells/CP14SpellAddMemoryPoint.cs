using Content.Shared._CP14.Skill;

namespace Content.Shared._CP14.MagicSpell.Spells;

public sealed partial class CP14SpellAddMemoryPoint : CP14SpellEffect
{
    [DataField]
    public float AddedPoints = 0.5f;

    [DataField]
    public float Limit = 6.5f;

    public override void Effect(EntityManager entManager, CP14SpellEffectBaseArgs args)
    {
        if (args.Target is null)
            return;

        var skillSys = entManager.System<CP14SharedSkillSystem>();

        skillSys.AddMemoryPoints(args.Target.Value, AddedPoints, Limit);
    }
}
