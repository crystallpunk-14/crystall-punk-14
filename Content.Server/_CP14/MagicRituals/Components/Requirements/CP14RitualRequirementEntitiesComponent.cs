using Content.Shared.Stacks;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.MagicRituals.Components.Requirements;

/// <summary>
///
/// </summary>
[RegisterComponent, Access(typeof(CP14RitualSystem))]
public sealed partial class CP14RitualRequirementEntitiesComponent : Component
{
    [DataField]
    public float CheckRange = 1f;

    /// <summary>
    /// Entities that should be located nearby, but which will not be spent.
    /// </summary>
    [DataField]
    public Dictionary<EntProtoId, int> RequiredEntities = new ();

    /// <summary>
    /// Entities to be placed next to each other, which will be spent.
    /// </summary>
    [DataField]
    public Dictionary<EntProtoId, int> ConsumedEntities = new ();

    [DataField]
    public Dictionary<ProtoId<StackPrototype>, int> RequiredStack = new();

    [DataField]
    public Dictionary<ProtoId<StackPrototype>, int> ConsumedStack = new();

    /// <summary>
    /// Effect appearing in place of used entities
    /// </summary>
    [DataField]
    public EntProtoId? VisualEffect;
}
