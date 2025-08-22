using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared._CP14.Vampire.Components;

[RegisterComponent]
[NetworkedComponent]
[AutoGenerateComponentState]
[Access(typeof(CP14SharedVampireSystem))]
public sealed partial class CP14VampireEssenceHolderComponent : Component
{
    [DataField, AutoNetworkedField]
    public FixedPoint2 Essence = 1f;
}
