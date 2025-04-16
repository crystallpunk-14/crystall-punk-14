using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Sponsor;

[Prototype("sponsorRole")]
public sealed partial class CP14SponsorRolePrototype : IPrototype
{
    [IdDataField] public string ID { get; } = string.Empty;

    [DataField(required: true)]
    public LocId Name = string.Empty;

    [DataField(required: true)]
    public string DiscordRoleId = string.Empty;
}

[Prototype("sponsorFeature")]
public sealed partial class CP14SponsorFeaturePrototype : IPrototype
{
    [IdDataField] public string ID { get; } = string.Empty;

    [DataField(required: true)]
    public HashSet<CP14SponsorRolePrototype> ForRoles = new();
}
