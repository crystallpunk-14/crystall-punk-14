using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicSpell.Spells;

public sealed partial class CP14SpellSpawnEntity : CP14SpellEffect
{
    [DataField]
    public List<EntProtoId> Spawns = new();

    [DataField]
    public string Key = "target";

    [DataField]
    public bool TryParent = false;

    //Я сделал хуйню
    public override void Effect(EntityManager entManager, CP14SpellEffectBaseArgs args)
    {
        EntityCoordinates? targetPos = null;
        EntityUid? targetEntity = null;
        TransformComponent? transformComponent = null;

        var transform = entManager.System<SharedTransformSystem>();
        switch (Key)
        {
            case "target":
                targetPos = args.Position;

                if (args.Target is not null && entManager.TryGetComponent(args.Target, out transformComponent))
                {
                    targetPos = transformComponent.Coordinates;
                    targetEntity = args.Target;
                }

                break;
            case "user":
                if (args.User is null || !entManager.TryGetComponent(args.User.Value, out transformComponent))
                    return;
                targetEntity = args.User;
                targetPos = transformComponent.Coordinates;
                break;
        }


        if (TryParent)
        {
            if (transformComponent is not null && targetEntity is not null)
            {
                foreach (var spawn in Spawns)
                {
                    var s = entManager.SpawnAtPosition(spawn, transformComponent.Coordinates);
                    transform.SetParent(s, targetEntity.Value);
                }
            }
        }
        else
        {
            if (targetPos is not null)
            {
                foreach (var spawn in Spawns)
                {
                    var s = entManager.SpawnAtPosition(spawn, targetPos.Value);
                }
            }
        }
    }
}
