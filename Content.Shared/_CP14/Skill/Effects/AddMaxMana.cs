using Content.Shared._CP14.MagicEnergy;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Skill.Effects;

public sealed partial class AddManaMax : CP14SkillEffect
{
    [DataField]
    public FixedPoint2 AdditionalMana = 0;
    public override void AddSkill(IEntityManager entManager, EntityUid target)
    {
        var magicSystem = entManager.System<SharedCP14MagicEnergySystem>();
        magicSystem.ChangeMaximumEnergy(target, AdditionalMana);
    }

    public override void RemoveSkill(IEntityManager entManager, EntityUid target)
    {
        var magicSystem = entManager.System<SharedCP14MagicEnergySystem>();
        magicSystem.ChangeMaximumEnergy(target, -AdditionalMana);
    }

    public override string? GetName(IEntityManager entMagager, IPrototypeManager protoManager)
    {
        return null;
    }

    public override string? GetDescription(IEntityManager entMagager, IPrototypeManager protoManager)
    {
        return Loc.GetString("cp14-skill-desc-add-mana", ("mana", AdditionalMana.ToString()));
    }
}
