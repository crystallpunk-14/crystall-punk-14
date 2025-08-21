using System.Text;
using Content.Shared._CP14.Skill.Prototypes;
using Content.Shared._CP14.Skill.Restrictions;
using Content.Shared._CP14.Workbench.Prototypes;
using Content.Shared.Construction.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Skill.Effects;

/// <summary>
/// This effect only exists for parsing the description.
/// </summary>
public sealed partial class UnlockConstructions : CP14SkillEffect
{
    public override void AddSkill(IEntityManager entManager, EntityUid target)
    {
        //
    }

    public override void RemoveSkill(IEntityManager entManager, EntityUid target)
    {
        //
    }

    public override string? GetName(IEntityManager entMagager, IPrototypeManager protoManager)
    {
        return null;
    }

    public override string? GetDescription(IEntityManager entMagager, IPrototypeManager protoManager, ProtoId<CP14SkillPrototype> skill)
    {
        var allRecipes = protoManager.EnumeratePrototypes<ConstructionPrototype>();

        var sb = new StringBuilder();
        sb.Append(Loc.GetString("cp14-skill-desc-unlock-constructions") + "\n");

        var affectedRecipes = new List<ConstructionPrototype>();
        foreach (var recipe in allRecipes)
        {
            foreach (var req in recipe.CP14Restrictions)
            {
                if (req is NeedPrerequisite prerequisite)
                {
                    if (prerequisite.Prerequisite == skill)
                    {
                        affectedRecipes.Add(recipe);
                        break;
                    }
                }
            }
        }
        foreach (var recipe in affectedRecipes)
        {
            sb.Append("- " + recipe.Name + "\n");
        }

        return sb.ToString();
    }
}
