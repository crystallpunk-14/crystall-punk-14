using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.FixedPoint;
using Content.Shared.Interaction;
using Content.Shared.Kitchen.Components;
using Content.Shared.Stacks;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Random;

namespace Content.Server._CP14.Alchemy;

public sealed partial class  CP14AlchemyExtractionSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedStackSystem _stackSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14MortarComponent, InteractUsingEvent>(OnInteractUsing);
    }

    private void OnInteractUsing(Entity<CP14MortarComponent> mortar, ref InteractUsingEvent args)
    {
        if (!TryComp<CP14PestleComponent>(args.Used, out var pestle))
            return;

        _audio.PlayPvs(pestle.HitSound, mortar);

        if (!_random.Prob(pestle.Probability))
            return;

        if (!TryComp<SolutionContainerManagerComponent>(mortar, out var solutionManagerComp))
            return;
        var solutionManager = new Entity<SolutionContainerManagerComponent?>(mortar, solutionManagerComp);

        if (!_solutionContainer.TryGetSolution(solutionManager, mortar.Comp.Solution, out var solutionEnt, out var solution))
            return;

        if (!_container.TryGetContainer(mortar, mortar.Comp.ContainerId, out var container))
            return;

        if (container.ContainedEntities.Count == 0)
            return;

        var ent = _random.Pick(container.ContainedEntities);

        var juiceSolution = CompOrNull<ExtractableComponent>(ent)?.JuiceSolution;
        if (juiceSolution is null)
            return;

        if (TryComp<StackComponent>(ent, out var stack))
        {
            var totalVolume = juiceSolution.Volume * stack.Count;
            if (totalVolume <= 0)
                return;

            // Maximum number of items we can process in the stack without going over AvailableVolume
            // We add a small tolerance, because floats are inaccurate.
            var fitsCount = (int) (stack.Count * FixedPoint2.Min(solution.AvailableVolume / totalVolume + 0.01, 1));
            if (fitsCount <= 0)
                return;

            // Make a copy of the solution to scale
            // Otherwise we'll actually change the volume of the remaining stack too
            var scaledSolution = new Solution(juiceSolution);
            scaledSolution.ScaleSolution(fitsCount);
            juiceSolution = scaledSolution;

            _stackSystem.SetCount(ent, stack.Count - fitsCount); // Setting to 0 will QueueDel
        }
        else
        {
            if (juiceSolution.Volume > solution.AvailableVolume)
                return;

            QueueDel(ent);
        }

        _solutionContainer.TryAddSolution(solutionEnt.Value, juiceSolution);
    }
}
