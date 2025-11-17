using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Species;

public sealed partial class SharedSpeciesSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypes = default!;

    /// <summary>
    /// Returns the list of requirements for a role, or null. May be altered by requirement overrides.
    /// </summary>
    public HashSet<JobRequirement>? GetSpeciesRequirements(SpeciesPrototype species)
    {
        return species.Requirements;
    }

    /// <inheritdoc cref="GetSpeciesRequirements(SpeciesPrototype)"/>
    public HashSet<JobRequirement>? GetSpeciesRequirements(ProtoId<SpeciesPrototype> speciesId)
    {
        return _prototypes.TryIndex(speciesId, out var job) ? GetSpeciesRequirements(job) : null;
    }
}
