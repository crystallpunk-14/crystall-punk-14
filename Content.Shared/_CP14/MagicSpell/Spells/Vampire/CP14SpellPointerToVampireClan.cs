using Content.Shared._CP14.Vampire.Components;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicSpell.Spells;

/// <summary>
/// Indicates all vampires within range belonging to the same faction as the caster. If inverted, it indicates enemy vampires.
/// </summary>
public sealed partial class CP14SpellPointerToVampireClan : CP14SpellEffect
{
    [DataField(required: true)]
    public EntProtoId PointerEntity;

    [DataField(required: true)]
    public float SearchRange = 60f;

    [DataField]
    public bool Inversed = false;

    public override void Effect(EntityManager entManager, CP14SpellEffectBaseArgs args)
    {
        var net = IoCManager.Resolve<INetManager>();
        if (net.IsClient)
            return;

        if (args.User is null)
            return;

        if (!entManager.TryGetComponent<CP14VampireComponent>(args.User.Value, out var vampireComponent))
            return;

        var lookup = entManager.System<EntityLookupSystem>();
        var transform = entManager.System<SharedTransformSystem>();

        var originPosition = transform.GetWorldPosition(args.User.Value);
        var originEntPosition = transform.GetMoverCoordinates(args.User.Value);

        var entitiesInRange = lookup.GetEntitiesInRange<CP14VampireComponent>(originEntPosition, SearchRange);
        foreach (var ent in entitiesInRange)
        {
            if (ent.Owner == args.User.Value)
                continue;

            if (!Inversed)
            {
                if (ent.Comp.Faction != vampireComponent.Faction)
                    continue;
            }
            else
            {
                if (ent.Comp.Faction == vampireComponent.Faction)
                    continue;
            }

            var targetPosition = transform.GetWorldPosition(ent);

            //Calculate the rotation
            Angle angle = new(targetPosition - originPosition);

            var pointer = entManager.Spawn(PointerEntity, new MapCoordinates(originPosition, transform.GetMapId(originEntPosition)));

            transform.SetWorldRotation(pointer, angle + Angle.FromDegrees(90));
        }
    }
}
