using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.GameTicking.Rules.Components;

[RegisterComponent, Access(typeof(CP14BloodMoonRule))]
public sealed partial class CP14BloodMoonRuleComponent : Component
{
    [DataField]
    public EntProtoId CurseRule = "CP14BloodMoonCurseRule";

    [DataField]
    public LocId StartAnnouncement = "cp14-bloodmoon-raising";

    [DataField]
    public Color? AnnouncementColor = Color.FromHex("#e32759");

    [DataField]
    public SoundSpecifier? AnnounceSound;
}
