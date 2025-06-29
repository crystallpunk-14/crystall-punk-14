using Content.Shared.Hands.EntitySystems;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicSpell.Spells;

public sealed partial class CP14SpellSpawnInHandEntity : CP14SpellEffect
{
    [DataField]
    public List<EntProtoId> Spawns = new();

    [DataField]
    public bool DeleteIfCantPickup = false;

    public override void Effect(EntityManager entManager, CP14SpellEffectBaseArgs args)
    {
        if (args.Target is null)
            return;

        if (!entManager.TryGetComponent<TransformComponent>(args.Target.Value, out var transformComponent))
            return;

        var handSystem = entManager.System<SharedHandsSystem>();

        foreach (var spawn in Spawns)
        {
            var item = entManager.PredictedSpawnAtPosition(spawn, transformComponent.Coordinates);
            if (!handSystem.TryPickupAnyHand(args.Target.Value, item) && DeleteIfCantPickup)
                entManager.QueueDeleteEntity(item);
        }
    }
}
