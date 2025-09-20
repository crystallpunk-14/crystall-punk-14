using Content.Shared.Roles;

// ReSharper disable once CheckNamespace
namespace Content.Shared.Humanoid.Prototypes;

public sealed partial class SpeciesPrototype
{
    /// <summary>
    ///     Requirements for the species in the character editor.
    /// </summary>
    [DataField]
    public HashSet<JobRequirement>? Requirements;
}
