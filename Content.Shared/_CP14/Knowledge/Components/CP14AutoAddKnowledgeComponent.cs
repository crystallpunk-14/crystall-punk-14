using Content.Shared._CP14.Knowledge.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Knowledge.Components;

/// <summary>
/// The ability to add a <see cref="CP14KnowledgePrototype"/> to an entity
/// and quickly teach it some skills.
/// </summary>
[RegisterComponent, Access(typeof(SharedCP14KnowledgeSystem))]
public sealed partial class CP14AutoAddKnowledgeComponent : Component
{
    [DataField]
    public List<ProtoId<CP14KnowledgePrototype>> Knowledge = [];
}
