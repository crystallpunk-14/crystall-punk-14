using Content.Shared.Materials;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Material;

[RegisterComponent]
public sealed partial class CP14MaterialComponent : Component
{
    [DataField(required: true)]
    public Dictionary<ProtoId<MaterialPrototype>, int> Materials = new();
}
