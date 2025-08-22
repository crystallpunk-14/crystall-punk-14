using System.Numerics;
using Content.Server.Destructible;
using Content.Server.Destructible.Thresholds.Behaviors;
using Content.Shared._CP14.Vampire.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._CP14.Vampire;

[Serializable]
[DataDefinition]
public sealed partial class CP14VampireAltarExplodeBehavior : IThresholdBehavior
{
    [DataField]
    public EntProtoId Essence = "CP14BloodEssence";

    [DataField]
    public float ExtractionPercentage = 0.5f;

    public void Execute(EntityUid owner, DestructibleSystem system, EntityUid? cause = null)
    {
        if(!system.EntityManager.TryGetComponent<TransformComponent>(owner, out var transform))
            return;

        if(!system.EntityManager.TryGetComponent<CP14VampireClanHeartComponent>(owner, out var clanHeart))
            return;

        var random = IoCManager.Resolve<IRobustRandom>();

        var collected = clanHeart.CollectedEssence;
        var spawnedEssence = MathF.Floor(collected.Float() * ExtractionPercentage);
        for (var i = 0; i < spawnedEssence; i++)
        {
            system.EntityManager.SpawnAtPosition(Essence, transform.Coordinates.Offset(new Vector2(random.NextFloat(-1f, 1f), random.NextFloat(-1f, 1f))));
        }
    }
}
