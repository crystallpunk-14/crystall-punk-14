using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Magic.Components;

/// <summary>
/// Creates a temporary entity that exists while the spell is cast, and disappears at the end. For visual special effects.
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedMagicSystem))]
public sealed partial class CP14MagicEffectCastingVisualComponent : Component
{
    [DataField]
    public EntityUid? SpawnedEntity;

    [DataField(required: true)]
    public EntProtoId Proto = default!;
}
