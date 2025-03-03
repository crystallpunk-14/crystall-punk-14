using Content.Server.Destructible;
using Content.Server.Fluids.EntitySystems;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Throwing;
using Robust.Shared.Audio.Systems;

namespace Content.Server._CP14.Chemistry.SmokeBomb;

public partial class CP14SmokeBombSystem : EntitySystem
{
    [Dependency] private readonly SmokeSystem _smoke = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14SmokeBombComponent, LandEvent>(OnLand, before: [typeof(DestructibleSystem)]);
    }

    private void OnLand(Entity<CP14SmokeBombComponent> ent, ref LandEvent args)
    {
        if (!TryComp<SolutionContainerManagerComponent>(ent, out var solutionManager))
            return;

        if (!_solution.TryGetSolution((ent, solutionManager), ent.Comp.Solution, out var solution))
            return;

        var position = Transform(ent).Coordinates;
        var spreadAmount = (int) solution.Value.Comp.Solution.Volume;

        if (spreadAmount <= 0)
            return;

        var smokeEnt = SpawnAtPosition(ent.Comp.SmokeProto, Transform(ent).Coordinates);
        _smoke.StartSmoke(smokeEnt, solution.Value.Comp.Solution, ent.Comp.Duration, spreadAmount);
        _audio.PlayPvs(ent.Comp.Sound, position);
    }
}
