using Robust.Shared.Prototypes;

namespace Content.Server._CP14.GameTicking.Rules.Components;

[RegisterComponent, Access(typeof(CP14BloodMoonCurseRule))]
public sealed partial class CP14BloodMoonCurseRuleComponent : Component
{
    [DataField]
    public LocId StartAnnouncement = "cp14-bloodmoon-start";

    [DataField]
    public LocId EndAnnouncement = "cp14-bloodmoon-end";

    [DataField]
    public Color? AnnouncementColor;

    [DataField]
    public TimeSpan EndStunDuration = TimeSpan.FromSeconds(60f);

    [DataField]
    public EntProtoId CurseEffect = "CP14ImpactEffectMagicSplitting";
}
