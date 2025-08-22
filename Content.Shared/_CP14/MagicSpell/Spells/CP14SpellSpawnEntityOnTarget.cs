using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicSpell.Spells;

public sealed partial class CP14SpellSpawnEntityOnTarget : CP14SpellEffect
{
    [DataField]
    public List<EntProtoId> Spawns = new();

    [DataField]
    public bool Clientside = false;

    public override void Effect(EntityManager entManager, CP14SpellEffectBaseArgs args)
    {
        EntityCoordinates? targetPoint = null;
        if (args.Position is not null)
            targetPoint = args.Position.Value;
        if (args.Target is not null && entManager.TryGetComponent<TransformComponent>(args.Target.Value, out var transformComponent))
            targetPoint = transformComponent.Coordinates;

        if (targetPoint is null)
            return;

        var netMan = IoCManager.Resolve<INetManager>();

        foreach (var spawn in Spawns)
        {
            if (Clientside)
            {
                if (!netMan.IsClient)
                    continue;

                entManager.SpawnAtPosition(spawn, targetPoint.Value);
            }
            else
            {
                entManager.PredictedSpawnAtPosition(spawn, targetPoint.Value);
            }
        }
    }
}
