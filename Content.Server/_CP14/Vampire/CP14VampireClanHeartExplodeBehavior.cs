using Content.Server.Destructible;
using Content.Server.Destructible.Thresholds.Behaviors;
using Content.Shared._CP14.Vampire.Components;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Vampire;

[Serializable]
[DataDefinition]
public sealed partial class CP14VampireAltarExplodeBehavior : IThresholdBehavior
{
    [DataField]
    public EntProtoId VFX = "CP14SkyLightningRed";

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

        var spawnedEssence = (int)MathF.Floor((float)clanHeart.CollectedEssence * ExtractionPercentage);
        for (var i = 0; i < spawnedEssence; i++)
        {
            system.EntityManager.SpawnAtPosition(Essence, transform.Coordinates);
        }

        system.EntityManager.SpawnAtPosition(VFX, transform.Coordinates);
    }
}
