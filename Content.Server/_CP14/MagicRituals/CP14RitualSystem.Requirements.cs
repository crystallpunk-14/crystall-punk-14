using Content.Server._CP14.MagicRituals.Components.Requirements;
using Content.Shared._CP14.MagicRitual;

namespace Content.Server._CP14.MagicRituals;

public sealed partial class CP14RitualSystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;

    private void InitializeRequirements()
    {
        SubscribeLocalEvent<CP14RitualRequirementEntitiesComponent, CP14RitualTriggerAttempt>(OnResourceRequirementCheck);
    }

    private void OnResourceRequirementCheck(Entity<CP14RitualRequirementEntitiesComponent> ent, ref CP14RitualTriggerAttempt args)
    {
        var entitiesAround = _lookup.GetEntitiesInRange(ent, ent.Comp.CheckRange, LookupFlags.Uncontained);
        

    }
}
