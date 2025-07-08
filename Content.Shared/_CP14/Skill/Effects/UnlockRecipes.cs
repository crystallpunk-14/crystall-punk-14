using System.Text;
using Content.Shared._CP14.Skill.Prototypes;
using Content.Shared._CP14.Workbench.Prototypes;
using Content.Shared._CP14.Workbench.Requirements;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Skill.Effects;

/// <summary>
/// This effect only exists for parsing the description.
/// </summary>
public sealed partial class UnlockRecipes : CP14SkillEffect
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
        var allRecipes = protoManager.EnumeratePrototypes<CP14WorkbenchRecipePrototype>();

        var sb = new StringBuilder();
        sb.Append(Loc.GetString("cp14-skill-desc-unlock-recipes") + "\n");

        var affectedRecipes = new List<CP14WorkbenchRecipePrototype>();
        foreach (var recipe in allRecipes)
        {
            foreach (var req in recipe.RequiredSkills)
            {
                if (req == skill)
                {
                    affectedRecipes.Add(recipe);
                    break;
                }
            }
        }
        foreach (var recipe in affectedRecipes)
        {
            if (!protoManager.TryIndex(recipe.Result, out var indexedResult))
                continue;
            sb.Append("- " + indexedResult.Name + "\n");
        }

        return sb.ToString();
    }
}
