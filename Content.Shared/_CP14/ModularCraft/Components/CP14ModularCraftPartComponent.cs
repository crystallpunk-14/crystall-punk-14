using Content.Shared._CP14.ModularCraft.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.ModularCraft.Components;

[RegisterComponent, Access(typeof(CP14SharedModularCraftSystem))]
public sealed partial class CP14ModularCraftPartComponent : Component
{
    [DataField(required: true)]
    public HashSet<ProtoId<CP14ModularCraftPartPrototype>> PossiblePartProto = new();
}
