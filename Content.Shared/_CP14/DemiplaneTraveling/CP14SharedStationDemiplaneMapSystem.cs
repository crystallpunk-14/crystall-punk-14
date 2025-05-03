using Robust.Shared.Serialization;

namespace Content.Shared._CP14.DemiplaneTraveling;

public abstract partial class CP14SharedStationDemiplaneMapSystem : EntitySystem
{
    public static bool CanEjectCoordinates(Dictionary<Vector2i,CP14DemiplaneMapNode> nodes, HashSet<(Vector2i, Vector2i)> edges, Vector2i key)
    {
        if (!nodes.TryGetValue(key, out var node))
            return false;

        if (node.Finished || node.Ejectable || node.Start)
            return false;

        //return false if no finished or start nodes near
        var near = new List<Vector2i>();
        foreach (var edge in edges)
        {
            var node1 = nodes[edge.Item1];
            var node2 = nodes[edge.Item2];

            if (node1 != node && node2 != node)
                continue;

            if (node1.Finished || node1.Start || node2.Finished || node2.Start)
            {
                return true;
            }
        }

        return false;
    }
}

[Serializable, NetSerializable]
public sealed class CP14DemiplaneMapEjectMessage(Vector2i position) : BoundUserInterfaceMessage
{
    public readonly Vector2i Position = position;
}

