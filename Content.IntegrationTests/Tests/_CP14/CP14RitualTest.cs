using System.Collections.Generic;
using System.Text;
using Content.Server._CP14.MagicRituals.Components.Triggers;
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

                    List<EntProtoId> allEdges = new();
                    List<EntProtoId> edgesToCheck = new();

                    foreach (var edge in phase.Edges)
                    {
                        edgesToCheck.Add(edge.Target);
                        allEdges.Add(edge.Target);
                    }

                    if (proto.TryGetComponent(out CP14RitualTriggerVoiceComponent? voiceTrigger, compFactory))
                    {
                        foreach (var trigger in voiceTrigger.Triggers)
                        {
                            Assert.That(trigger.Speakers > 0, $"{proto} has voice trigger edge with less than 1 speaker!");
                            Assert.That(allEdges.Contains(trigger.TargetPhase), $"{proto} have voice trigger to {trigger.TargetPhase}, but this phase edge do not exist!");

                            if (edgesToCheck.Contains(trigger.TargetPhase))
                                edgesToCheck.Remove(trigger.TargetPhase);
                        }
                    }

                    if (proto.TryGetComponent(out CP14RitualTriggerTimerComponent? timerTrigger, compFactory))
                    {
                        Assert.That(allEdges.Contains(timerTrigger.NextPhase), $"{proto} have timer trigger to {timerTrigger.NextPhase}, but this phase edge do not exist!");

                        if (edgesToCheck.Contains(timerTrigger.NextPhase))
                            edgesToCheck.Remove(timerTrigger.NextPhase);
                    }

                    var sb = new StringBuilder();
                    foreach (var leftEdge in edgesToCheck)
                    {
                        sb.Append(leftEdge.Id + "\n");
                    }
                    Assert.That(edgesToCheck.Count == 0, $"The following {proto} edges have no triggers: \n {sb.ToString()}");
                }
            });
        });

        await pair.CleanReturnAsync();
    }
}
