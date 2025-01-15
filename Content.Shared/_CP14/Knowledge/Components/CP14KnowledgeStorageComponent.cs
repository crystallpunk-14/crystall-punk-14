using Content.Shared._CP14.Knowledge.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Knowledge.Components;

/// <summary>
/// a list of skills learned by this entity
/// </summary>
[RegisterComponent, Access(typeof(SharedCP14KnowledgeSystem))]
public sealed partial class CP14KnowledgeStorageComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public HashSet<ProtoId<CP14KnowledgePrototype>> Knowledges { get; private set; }= new();
}
