using Content.Server._CP14.MagicEnergy;
using Content.Server.Audio;
using Content.Shared._CP14.MagicEnergy.Components;
using Content.Shared.Audio;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Server._CP14.Alchemy;

public sealed partial class CP14SolutionNormalizerSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly CP14MagicEnergyCrystalSlotSystem _magicSlot = default!;
    [Dependency] private readonly AmbientSoundSystem _ambient = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CP14SolutionNormalizerComponent,CP14SlotCrystalPowerChangedEvent>(OnSlotPowerChanged);
    }

    private void OnSlotPowerChanged(Entity<CP14SolutionNormalizerComponent> ent, ref CP14SlotCrystalPowerChangedEvent args)
    {
        if (TryComp<AmbientSoundComponent>(ent, out var ambient))
        {
            _ambient.SetAmbience(ent, args.Powered);
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CP14SolutionNormalizerComponent, SolutionContainerManagerComponent>();
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

            Dictionary<ReagentId, FixedPoint2> affect = new();
            foreach (var (id, quantity) in solution.Contents)
            {
                FixedPoint2 roundedQuantity = Math.Floor((float) quantity / normalizer.Factor) * normalizer.Factor;

                var leakQuantity = quantity - roundedQuantity;

                if (leakQuantity == 0) continue;

                if (quantity - normalizer.LeakageQuantity < roundedQuantity)
                    affect.Add(id, leakQuantity);
                else
                    affect.Add(id, normalizer.LeakageQuantity);
            }

            if (affect.Count > 0)
            {
                //Telegraphy
                _audio.PlayPvs(normalizer.NormalizeSound, uid);

                foreach (var (id, count) in affect)
                {
                    _solutionContainer.RemoveReagent(solutionEnt.Value, id, count);
                }
            }
        }
    }
}
