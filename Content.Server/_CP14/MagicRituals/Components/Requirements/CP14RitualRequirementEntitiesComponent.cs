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

    [DataField]
    public Dictionary<EntProtoId, int> RequiredEntities = new ();

    [DataField]
    public Dictionary<ProtoId<StackPrototype>, int> RequiredStack = new();

    [DataField]
    public bool ConsumeResource = false;

    /// <summary>
    /// Effect appearing in place of used entities
    /// </summary>
    [DataField]
    public EntProtoId? VisualEffect = "CP14DustEffect";
}
