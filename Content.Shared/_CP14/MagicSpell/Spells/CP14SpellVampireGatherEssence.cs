using Content.Shared._CP14.Skill;
using Content.Shared._CP14.Skill.Prototypes;
using Content.Shared._CP14.Vampire;
using Content.Shared.FixedPoint;
using Content.Shared.Humanoid;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.SSDIndicator;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicSpell.Spells;

public sealed partial class CP14SpellVampireGatherEssence : CP14SpellEffect
{
    [DataField]
    public FixedPoint2 Amount = 0.1f;

    [DataField]
    public ProtoId<CP14SkillPointPrototype> SkillPointType = "Blood";

    public override void Effect(EntityManager entManager, CP14SpellEffectBaseArgs args)
    {
        if (args.Target is null)
            return;

        if (args.User is null)
            return;

        if (!entManager.HasComponent<HumanoidAppearanceComponent>(args.Target.Value))
            return;

        if (entManager.HasComponent<CP14VampireComponent>(args.Target.Value))
            return;

        if (entManager.TryGetComponent<MobStateComponent>(args.Target.Value, out var mobState) && mobState.CurrentState == MobState.Alive)
            return;

        if (entManager.TryGetComponent<SSDIndicatorComponent>(args.Target.Value, out var ssd) && ssd.IsSSD)
            return;

        var skillSys = entManager.System<CP14SharedSkillSystem>();
        skillSys.AddSkillPoints(args.User.Value, SkillPointType, Amount);
    }
}
