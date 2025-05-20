using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicSpell.Spells;

public sealed partial class CP14SpellPointerToAlive : CP14SpellEffect
{
    [DataField(required: true)]
    public EntProtoId PointerEntity;

    [DataField(required: true)]
    public float SearchRange = 30f;

    public override void Effect(EntityManager entManager, CP14SpellEffectBaseArgs args)
    {
        if (args.User is null)
            return;

        var lookup = entManager.System<EntityLookupSystem>();
        var mobStateSys = entManager.System<MobStateSystem>();
        var transform = entManager.System<SharedTransformSystem>();

        var originPosition = transform.GetWorldPosition(args.User.Value);
        var originEntPosition = transform.GetMoverCoordinates(args.User.Value);

        var entitiesInRange = lookup.GetEntitiesInRange<MobStateComponent>(originEntPosition, SearchRange);
        foreach (var ent in entitiesInRange)
        {
            if (ent.Owner == args.User.Value)
                continue;

            if (mobStateSys.IsDead(ent))
                continue;

            var targetPosition = transform.GetWorldPosition(ent);

            //Calculate the rotation
            Angle angle = new(targetPosition - originPosition);

            var pointer = entManager.Spawn(PointerEntity, new MapCoordinates(originPosition, transform.GetMapId(originEntPosition)));

            transform.SetWorldRotation(pointer, angle + Angle.FromDegrees(90));
        }
    }
}
