using Content.Server._CP14.Objectives.Systems;
using Robust.Shared.Utility;

namespace Content.Server._CP14.Objectives.Components;

[RegisterComponent, Access(typeof(CP14CurrencyCollectConditionSystem))]
public sealed partial class CP14CurrencyStoredConditionComponent : Component
{
    [DataField]
    public int Currency = 1000;

    [DataField(required: true)]
    public LocId ObjectiveText;

    [DataField(required: true)]
    public LocId ObjectiveDescription;

    [DataField(required: true)]
    public SpriteSpecifier ObjectiveSprite;
}
