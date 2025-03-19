using Robust.Shared.Prototypes;

namespace Content.Server._CP14.MagicSpell;

[RegisterComponent]
public sealed partial class CP14AutoLearnActionComponent : Component
{
    [DataField(required: true)]
    public HashSet<EntProtoId> Actions = new();
}
