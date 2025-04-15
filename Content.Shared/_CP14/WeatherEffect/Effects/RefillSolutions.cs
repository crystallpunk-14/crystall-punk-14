using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared._CP14.WeatherEffect.Effects;

public sealed partial class RefillSolutions : CP14WeatherEffect
{
    [DataField(required: true)]
    public Dictionary<ProtoId<ReagentPrototype>, float> Reagents = new();

    public override void ApplyEffect(IEntityManager entManager, IRobustRandom random, EntityUid target)
    {
        if (!random.Prob(Prob))
            return;

        if (!entManager.TryGetComponent<CP14WeatherRefillableComponent>(target, out var refillable))
            return;

        var solutionSystem = entManager.System<SharedSolutionContainerSystem>();

        solutionSystem.TryGetSolution(target, refillable.Solution, out var ent, out var solution);

        if (ent is null)
            return;

        foreach (var r in Reagents)
        {
            solutionSystem.TryAddReagent(ent.Value, r.Key, r.Value);
        }
    }
}
