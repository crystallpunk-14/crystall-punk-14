using Content.Server._CP14.ModularCraft;
using Content.Server.Destructible;
using Content.Server.Destructible.Thresholds.Behaviors;
using Content.Server.Fluids.EntitySystems;
using Content.Shared.Audio;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Chemistry.SmokeBomb;

[Serializable]
[DataDefinition]
public sealed partial class CP14AreaEffectBehavior : IThresholdBehavior
{
    [DataField]
    public string? Solution;

    [DataField]
    public EntProtoId SmokeProto = "CP14Mist";

    [DataField]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/Effects/smoke.ogg");

    [DataField]
    public float Duration = 10;

    public void Execute(EntityUid owner, DestructibleSystem system, EntityUid? cause = null)
    {
        var solutionContainerSystem = system.EntityManager.System<SharedSolutionContainerSystem>();

        if (!solutionContainerSystem.TryGetSolution(owner, Solution, out _, out var solution))
            return;

        var smokeSystem = system.EntityManager.System<SmokeSystem>();
        var audio = system.EntityManager.System<SharedAudioSystem>();

        var coordinates = system.EntityManager.GetComponent<TransformComponent>(owner).Coordinates;
        var ent = system.EntityManager.SpawnAtPosition(SmokeProto, coordinates);

        smokeSystem.StartSmoke(ent, solution, Duration, (int) solution.Volume);
        audio.PlayPvs(Sound, coordinates, AudioHelpers.WithVariation(0.125f));
    }
}
