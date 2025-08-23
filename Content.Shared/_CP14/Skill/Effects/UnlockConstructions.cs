using System.Text;
using Content.Shared._CP14.Skill.Prototypes;
using Content.Shared._CP14.Skill.Restrictions;
using Content.Shared.Construction;
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
        foreach (var constructionProto in affectedRecipes)
        {
            if (!protoManager.TryIndex(constructionProto.Graph, out var graphProto))
                continue;

            if (constructionProto.TargetNode is not { } targetNodeId)
                continue;

            if (!graphProto.Nodes.TryGetValue(targetNodeId, out var targetNode))
                continue;

            // Recursion is for wimps.
            var stack = new Stack<ConstructionGraphNode>();
            stack.Push(targetNode);


            do
            {
                var node = stack.Pop();

                // We try to get the id of the target prototype, if it fails, we try going through the edges.
                if (node.Entity.GetId(null, null, new(entMagager)) is not { } entityId)
                {
                    // If the stack is not empty, there is a high probability that the loop will go to infinity.
                    if (stack.Count == 0)
                    {
                        foreach (var edge in node.Edges)
                        {
                            if (graphProto.Nodes.TryGetValue(edge.Target, out var graphNode))
                                stack.Push(graphNode);
                        }
                    }

                    continue;
                }

                // If we got the id of the prototype, we exit the “recursion” by clearing the stack.
                stack.Clear();

                if (!protoManager.TryIndex(entityId, out var proto))
                    continue;

                sb.Append("- " + Loc.GetString(proto.Name) + "\n");

            } while (stack.Count > 0);
        }

        return sb.ToString();
    }
}
