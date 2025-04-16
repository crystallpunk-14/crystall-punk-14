using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Sponsor;

[Prototype("sponsorRole")]
public sealed partial class CP14SponsorRolePrototype : IPrototype
{
    [IdDataField] public string ID { get; } = string.Empty;

    [DataField(required: true)]
    public string Name = string.Empty;

    [DataField(required: true)]
    public string DiscordRoleId = string.Empty;

    [DataField]
    public Color? ColorOOC = null;

    [DataField]
    public float ColorPriority = 0;
}

[Prototype("sponsorFeature")]
public sealed partial class CP14SponsorFeaturePrototype : IPrototype
{
    [IdDataField] public string ID { get; } = string.Empty;

    [DataField(required: true)]
    public HashSet<CP14SponsorRolePrototype> ForRoles = new();
}
