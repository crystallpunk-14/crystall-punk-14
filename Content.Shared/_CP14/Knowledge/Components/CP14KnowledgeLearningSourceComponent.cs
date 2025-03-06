using Content.Shared._CP14.Knowledge.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Knowledge.Components;

/// <summary>
/// Allows new knowledge to be learnt through interactions with an object.
/// </summary>
[RegisterComponent, Access(typeof(SharedCP14KnowledgeSystem))]
public sealed partial class CP14KnowledgeLearningSourceComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public HashSet<ProtoId<CP14KnowledgePrototype>> Knowledge { get; private set; } = [];

    [DataField]
    public TimeSpan DoAfter = TimeSpan.FromSeconds(5f);
}
