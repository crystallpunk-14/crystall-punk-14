using System.Text;
using Content.Shared._CP14.MagicManacostModify;
using Content.Shared._CP14.MagicRitual.Prototypes;
using Content.Shared._CP14.Skill.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Skill.Effects;

public sealed partial class ModifyManacost : CP14SkillEffect
{
    [DataField]
    public FixedPoint2 Global = 0f;

    [DataField]
    public Dictionary<ProtoId<CP14MagicTypePrototype>, FixedPoint2> Modifiers = new();

    public override void AddSkill(IEntityManager entManager, EntityUid target)
    {
        entManager.EnsureComponent<CP14MagicManacostModifyComponent>(target, out var magicEffectManaCost);

        foreach (var (magicType, modifier) in Modifiers)
        {
            if (!magicEffectManaCost.Modifiers.ContainsKey(magicType))
                magicEffectManaCost.Modifiers.Add(magicType, 1 + modifier);
            else
                magicEffectManaCost.Modifiers[magicType] += modifier;
        }
        magicEffectManaCost.GlobalModifier += Global;
    }

    public override void RemoveSkill(IEntityManager entManager, EntityUid target)
    {
        entManager.EnsureComponent<CP14MagicManacostModifyComponent>(target, out var magicEffectManaCost);

        foreach (var (magicType, modifier) in Modifiers)
        {
            if (!magicEffectManaCost.Modifiers.ContainsKey(magicType))
                continue;

            magicEffectManaCost.Modifiers[magicType] -= modifier;
            if (magicEffectManaCost.Modifiers[magicType] <= 0)
                magicEffectManaCost.Modifiers.Remove(magicType);
        }
        magicEffectManaCost.GlobalModifier -= Global;
    }

    public override string? GetName(IEntityManager entMagager, IPrototypeManager protoManager)
    {
        return null;
    }

    public override string? GetDescription(IEntityManager entMagager, IPrototypeManager protoManager, ProtoId<CP14SkillPrototype> skill)
    {
        var sb = new StringBuilder();
        sb.Append(Loc.GetString("cp14-clothing-magic-examine")+"\n");

        if (Global != 0)
        {

            var plus = (float)Global > 0 ? "+" : "";
            sb.Append(
                $"{Loc.GetString("cp14-clothing-magic-global")}: {plus}{MathF.Round((float)Global * 100, MidpointRounding.AwayFromZero)}%\n");
        }

        foreach (var modifier in Modifiers)
        {
            if (modifier.Value == 0)
                continue;

            var plus = modifier.Value > 1 ? "+" : "";
            var indexedType = protoManager.Index(modifier.Key);
            sb.Append($"- [color={indexedType.Color.ToHex()}]{Loc.GetString(indexedType.Name)}[/color]: {plus}{modifier.Value*100}%");
        }

        return sb.ToString();
    }
}
