using Content.Shared._CP14.MagicRitual;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests._CP14;

#nullable enable

[TestFixture]
public sealed class CP14RitualTest
{

    /// <summary>
    /// States that all edges of the ritual phase have triggers.
    /// </summary>
    [Test]
    public async Task RitualHasAllTriggersTest()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;

        var compFactory = server.ResolveDependency<IComponentFactory>();
        var protoManager = server.ResolveDependency<IPrototypeManager>();

        await server.WaitAssertion(() =>
        {
            Assert.Multiple(() =>
            {
                foreach (var proto in protoManager.EnumeratePrototypes<EntityPrototype>())
                {
                    if (!proto.TryGetComponent(out CP14MagicRitualPhaseComponent? phase, compFactory))
                        continue;

                    if (phase.DeadEnd)
                    {
                        Assert.That(phase.Edges.Count == 0, $"{proto} is a ritual node, but has no paths to other nodes. Either add deadEnd = true, or add paths to other nodes.");
                    }
                    else
                    {
                        Assert.That(phase.Edges.Count > 0, $"{proto} is a deadEnd ritual node, but has {phase.Edges.Count} edges! Remove all edges, or make it a non dead-end node");
                    }

                    foreach (var edge in phase.Edges)
                    {
                        Assert.That(edge.Triggers.Count > 0, $"{{proto}} is ritual node, but edge to {edge.Target} has no triggers and cannot be activated.");
                    }
                }
            });
        });

        await pair.CleanReturnAsync();
    }
}
