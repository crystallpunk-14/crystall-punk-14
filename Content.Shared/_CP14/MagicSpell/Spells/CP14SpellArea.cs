using Content.Shared.Whitelist;
using Robust.Shared.Map;

namespace Content.Shared._CP14.MagicSpell.Spells;

public sealed partial class CP14SpellArea : CP14SpellEffect
{
    [DataField(required: true)]
    public List<CP14SpellEffect> Effects { get; set; } = new();

    [DataField]
    public EntityWhitelist? Whitelist;

    /// <summary>
    /// How many entities can be subject to EntityEffect? Leave 0 to remove the restriction.
    /// </summary>
    [DataField]
    public int MaxTargets = 0;

    [DataField(required: true)]
    public float Range = 1f;

    [DataField]
    public bool AffectCaster = false;

    public override void Effect(EntityManager entManager, CP14SpellEffectBaseArgs args)
    {
        EntityCoordinates? targetPoint = null;

        if (args.Target is not null &&
            entManager.TryGetComponent<TransformComponent>(args.Target.Value, out var transformComponent))
            targetPoint = transformComponent.Coordinates;
        else if (args.Position is not null)
            targetPoint = args.Position;

        if (targetPoint is null)
            return;

        var lookup = entManager.System<EntityLookupSystem>();
        var whitelist = entManager.System<EntityWhitelistSystem>();

        var entitiesAround = lookup.GetEntitiesInRange(targetPoint.Value, Range, LookupFlags.Uncontained);

        var count = 0;
        foreach (var entity in entitiesAround)
        {
            if (entity == args.User && !AffectCaster)
                continue;

            if (Whitelist is not null && !whitelist.IsValid(Whitelist, entity))
                continue;

            foreach (var effect in Effects)
            {
                effect.Effect(entManager, new CP14SpellEffectBaseArgs(args.User, null, entity,  targetPoint));
            }

            count++;

            if (MaxTargets > 0 && count >= MaxTargets)
                break;
        }
    }
}
