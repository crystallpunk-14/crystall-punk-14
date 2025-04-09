
using Content.Shared._CP14.LockKey;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.LockKey;

[RegisterComponent]
public sealed partial class CP14AbstractKeyComponent : Component
{
    [DataField(required: true)]
    public ProtoId<CP14LockGroupPrototype> Group = default;

    [DataField]
    public bool DeleteOnFailure = true;
}
