using System.Diagnostics.CodeAnalysis;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicRituals.Graph;

[Prototype("ritualGraph")]
public sealed partial class CP14RitualGraphPrototype : IPrototype
{
    private readonly Dictionary<string, CP14RitualGraphNode> _nodes = new();
    private readonly Dictionary<(string, string), CP14RitualGraphNode[]?> _paths = new();
    private readonly Dictionary<string, Dictionary<CP14RitualGraphNode, CP14RitualGraphNode?>> _pathfinding = new();

    [ViewVariables]
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public string? Start { get; private set; }

    [DataField]
    public List<CP14RitualGraphNode> Graph = new();

    public bool TryPath(string startNode, string finishNode, [NotNullWhen(true)] out CP14RitualGraphNode[]? path)
    {
        return (path = Path(startNode, finishNode)) != null;
    }

    public string[]? PathId(string startNode, string finishNode)
    {
        if (Path(startNode, finishNode) is not {} path)
            return null;

        var nodes = new string[path.Length];

        for (var i = 0; i < path.Length; i++)
        {
            nodes[i] = path[i].Name;
        }

        return nodes;
    }

    public CP14RitualGraphNode[]? Path(string startNode, string finishNode)
    {
        var tuple = (startNode, finishNode);

        if (_paths.ContainsKey(tuple))
            return _paths[tuple];

        // Get graph given the current start.

        Dictionary<CP14RitualGraphNode, CP14RitualGraphNode?> pathfindingForStart;
        if (_pathfinding.ContainsKey(startNode))
        {
            pathfindingForStart = _pathfinding[startNode];
        }
        else
        {
            pathfindingForStart = _pathfinding[startNode] = PathsForStart(startNode);
        }

        // Follow the chain backwards.

        var start = _nodes[startNode];
        var finish = _nodes[finishNode];

        var current = finish;
        var path = new List<CP14RitualGraphNode>();
        while (current != start)
        {
            // No path.
            if (current == null || !pathfindingForStart.ContainsKey(current))
            {
                // We remember this for next time.
                _paths[tuple] = null;
                return null;
            }

            path.Add(current);

            current = pathfindingForStart[current];
        }

        path.Reverse();
        return _paths[tuple] = path.ToArray();
    }

    /// <summary>
    ///     Uses breadth first search for pathfinding.
    /// </summary>
    /// <param name="start"></param>
    private Dictionary<CP14RitualGraphNode, CP14RitualGraphNode?> PathsForStart(string start)
    {
        // TODO: Make this use A* or something, although it's not that important.
        var startNode = _nodes[start];

        var frontier = new Queue<CP14RitualGraphNode>();
        var cameFrom = new Dictionary<CP14RitualGraphNode, CP14RitualGraphNode?>();

        frontier.Enqueue(startNode);
        cameFrom[startNode] = null;

        while (frontier.Count != 0)
        {
            var current = frontier.Dequeue();
            foreach (var edge in current.Edges)
            {
                var edgeNode = _nodes[edge.Target];
                if(cameFrom.ContainsKey(edgeNode)) continue;
                frontier.Enqueue(edgeNode);
                cameFrom[edgeNode] = current;
            }
        }

        return cameFrom;
    }
}
