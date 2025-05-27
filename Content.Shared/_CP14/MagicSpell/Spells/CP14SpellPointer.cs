using Content.Shared.Whitelist;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicSpell.Spells;

public sealed partial class CP14SpellPointer : CP14SpellEffect
{
    [DataField(required: true)]
    public EntProtoId PointerEntity;

    [DataField(required: true)]
    public EntityWhitelist? Whitelist;

    [DataField]
    public EntityWhitelist? Blacklist = null;

    [DataField(required: true)]
    public float SearchRange = 30f;

    public override void Effect(EntityManager entManager, CP14SpellEffectBaseArgs args)
    {
        if (args.User is null)
            return;

        var lookup = entManager.System<EntityLookupSystem>();
        var whitelistSys = entManager.System<EntityWhitelistSystem>();
        var transform = entManager.System<SharedTransformSystem>();

        var originPosition = transform.GetWorldPosition(args.User.Value);
        var originEntPosition = transform.GetMoverCoordinates(args.User.Value);

        var entitiesInRange = lookup.GetEntitiesInRange<TransformComponent>(originEntPosition, SearchRange);
        foreach (var ent in entitiesInRange)
        {
            if (ent.Owner == args.User.Value)
                continue;

            if (!whitelistSys.CheckBoth(ent, Blacklist, Whitelist))
                continue;

            var targetPosition = transform.GetWorldPosition(ent.Comp);

            //Calculate the rotation
            Angle angle = new(targetPosition - originPosition);

            var pointer = entManager.Spawn(PointerEntity, new MapCoordinates(originPosition, transform.GetMapId(originEntPosition)));

            transform.SetWorldRotation(pointer, angle + Angle.FromDegrees(90));
        }
    }
}
