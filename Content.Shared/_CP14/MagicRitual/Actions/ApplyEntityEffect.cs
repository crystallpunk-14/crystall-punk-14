using System.Text;
using Content.Shared.EntityEffects;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicRitual.Actions;

/// <summary>
/// Filters the nearest X entities by whitelist and applies the specified EntityEffects on them
/// </summary>
public sealed partial class ApplyEntityEffect : CP14RitualAction
{
    [DataField]
    public float CheckRange = 1f;

    [DataField]
    public EntityWhitelist? Whitelist;

    [DataField]
    public LocId? WhitelistDesc;

    [DataField(required: true, serverOnly: true)]
    public List<EntityEffect> Effects = new();

    [DataField]
    public int MaxEntities = 1;

    public override string? GetGuidebookEffectDescription(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        var sb = new StringBuilder();

        sb.Append(Loc.GetString("cp14-ritual-range", ("range", CheckRange)) + " ");
        sb.Append(Loc.GetString("cp14-ritual-effect-apply-effect", ("count", MaxEntities), ("range", CheckRange)));
        sb.Append("\n");

        if (WhitelistDesc is not null)
        {
            sb.Append(Loc.GetString(WhitelistDesc));
            sb.Append("\n");
        }

        foreach (var effect in Effects)
        {
            sb.Append("- " + effect.GuidebookEffectDescription(prototype, entSys) + "\n");
        }
        sb.Append("\n");

        return sb.ToString();
    }

    public override void Effect(EntityManager entManager, SharedTransformSystem _transform, Entity<CP14MagicRitualPhaseComponent> phase)
    {
        var _lookup = entManager.System<EntityLookupSystem>();
        var _whitelist = entManager.System<EntityWhitelistSystem>();

        var entitiesAround = _lookup.GetEntitiesInRange(phase, CheckRange, LookupFlags.Uncontained);

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
