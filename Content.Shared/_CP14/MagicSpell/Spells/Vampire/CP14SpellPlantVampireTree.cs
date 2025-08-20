using Content.Shared._CP14.Vampire.Components;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicSpell.Spells;

public sealed partial class CP14SpellSpawnVampireTree : CP14SpellEffect
{
    public override void Effect(EntityManager entManager, CP14SpellEffectBaseArgs args)
    {
        if (args.User is null)
            return;

        EntityCoordinates? targetPoint = null;
        if (args.Position is not null)
            targetPoint = args.Position.Value;
        if (args.Target is not null && entManager.TryGetComponent<TransformComponent>(args.Target.Value, out var transformComponent))
            targetPoint = transformComponent.Coordinates;

        if (targetPoint is null)
            return;

        if (!entManager.TryGetComponent<CP14VampireComponent>(args.User.Value, out var vampireComponent))
            return;
        var protoMan = IoCManager.Resolve<IPrototypeManager>();

        if (!protoMan.TryIndex(vampireComponent.Faction, out var indexedFaction))
            return;

        entManager.PredictedSpawnAtPosition(indexedFaction.MotherTreeProto, targetPoint.Value);
    }
}
