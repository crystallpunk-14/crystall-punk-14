using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Actions.Components;

/// <summary>
/// Creates a temporary entity that exists while the spell is cast, and disappears at the end. For visual special effects.
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedActionSystem))]
public sealed partial class CP14ActionDoAfterVisualsComponent : Component
{
    [DataField]
    public EntityUid? SpawnedEntity;

    [DataField(required: true)]
    public EntProtoId Proto = default!;
}
