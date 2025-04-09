using Content.Server._CP14.Objectives.Systems;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server._CP14.Objectives.Components;

/// <summary>
/// The player must be the richest among the players among the specified list of roles
/// </summary>
[RegisterComponent, Access(typeof(CP14RichestJobConditionSystem))]
public sealed partial class CP14RichestJobConditionComponent : Component
{
    [DataField(required: true)]
    public ProtoId<JobPrototype> Job;

    [DataField(required: true)]
    public LocId ObjectiveText;

    [DataField(required: true)]
    public LocId ObjectiveDescription;

    [DataField(required: true)]
    public SpriteSpecifier ObjectiveSprite;
}
