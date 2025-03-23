using Content.Shared._CP14.Workbench.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.WorkbenchKnowledge.Components;

/// <summary>
/// A list of <see cref="CP14WorkbenchRecipePrototype"/> learned by this entity.
/// </summary>
[RegisterComponent, Access(typeof(SharedCP14WorkbenchKnowledgeSystem)), NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CP14WorkbenchKnowledgeStorageComponent : Component
{
    [DataField, AutoNetworkedField]
    public HashSet<ProtoId<CP14WorkbenchRecipePrototype>> Recipes = new();
}
