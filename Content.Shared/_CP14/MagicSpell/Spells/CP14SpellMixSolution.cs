using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reaction;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicSpell.Spells;

public sealed partial class CP14SpellMixSolution : CP14SpellEffect
{
    [DataField]
    public List<ProtoId<MixingCategoryPrototype>> ReactionTypes = default!;

    public override void Effect(EntityManager entManager, CP14SpellEffectBaseArgs args)
    {
        if (args.Target is null)
            return;

        if (!entManager.TryGetComponent<SolutionContainerManagerComponent>(args.Target.Value, out var solutionContainerManager))
            return;

        if (!entManager.TryGetComponent<MixableSolutionComponent>(args.Target.Value, out var mixableSolution))
            return;

        var solutionContainerSystem = entManager.System<SharedSolutionContainerSystem>();

        if (!solutionContainerSystem.TryGetSolution((args.Target.Value, solutionContainerManager), mixableSolution.Solution, out var solution))
            return;

        var reactionSystem = entManager.System<ChemicalReactionSystem>();

        reactionSystem.FullyReactSolution(solution.Value, new ReactionMixerComponent { ReactionTypes = ReactionTypes });
    }
}
