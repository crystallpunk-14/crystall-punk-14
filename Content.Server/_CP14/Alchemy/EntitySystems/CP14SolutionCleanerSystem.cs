using Content.Server._CP14.Alchemy.Components;
using Content.Server._CP14.MagicEnergy;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Server._CP14.Alchemy.EntitySystems;

public sealed partial class CP14SolutionCleanerSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly CP14MagicEnergyCrystalSlotSystem _magicSlot = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CP14SolutionCleanerComponent, SolutionContainerManagerComponent>();
        while (query.MoveNext(out var uid, out var normalizer, out var containerManager))
        {
            if (_timing.CurTime <= normalizer.NextUpdateTime)
                continue;

            if (!_magicSlot.HasEnergy(uid, 1))
                continue;

            normalizer.NextUpdateTime = _timing.CurTime + normalizer.UpdateFrequency;

            var solutionManager = new Entity<SolutionContainerManagerComponent?>(uid, containerManager);

            if (!_solutionContainer.TryGetSolution(solutionManager,
                    normalizer.Solution,
                    out var solutionEnt,
                    out var solution))
                continue;

            if (solution.Volume == 0)
                continue;

            var minQuantity = FixedPoint2.MaxValue;
            ReagentId? reagentId = null;
            foreach (var (id, quantity) in solution.Contents)
            {
                if (quantity < minQuantity)
                {
                    reagentId = id;
                    minQuantity = quantity;
                }
            }

            if (reagentId == null)
                continue;

            _solutionContainer.RemoveReagent(solutionEnt.Value, reagentId.Value, normalizer.LeakageQuantity);
            _audio.PlayPvs(normalizer.NormalizeSound, uid);
        }
    }
}
