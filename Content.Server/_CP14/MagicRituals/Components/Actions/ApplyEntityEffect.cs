using Content.Shared.EntityEffects;
using Content.Shared.Whitelist;

namespace Content.Server._CP14.MagicRituals.Components.Actions;

public sealed partial class ApplyEntityEffect : CP14RitualAction
{
    [DataField]
    public float CheckRange = 1f;

    [DataField]
    public EntityWhitelist? Whitelist = null;

    [DataField(required: true)]
    public List<EntityEffect> Effects = new();

    [DataField]
    public int MaxEntities = 1;

    public override void Effect(EntityManager entManager, EntityUid phaseEnt)
    {
        var _lookup = entManager.System<EntityLookupSystem>();
        var _transform = entManager.System<SharedTransformSystem>();
        var _whitelist = entManager.System<EntityWhitelistSystem>();


        var entitiesAround = _lookup.GetEntitiesInRange(phaseEnt, CheckRange, LookupFlags.Uncontained);

        var count = 0;
        foreach (var entity in entitiesAround)
        {
            if (Whitelist is not null && !_whitelist.IsValid(Whitelist, entity))
                continue;

            foreach (var effect in Effects)
            {
                effect.Effect(new EntityEffectBaseArgs(entity, entManager));
            }

            entManager.Spawn(VisualEffect, _transform.GetMapCoordinates(entity));
            count++;

            if (count >= MaxEntities)
                break;
        }
    }
}
