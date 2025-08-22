using Robust.Shared.GameStates;

namespace Content.Shared._CP14.Vampire.Components;

/// <summary>
/// increases the amount of blood essence extracted if the victim is strapped to the altar
/// </summary>
[RegisterComponent]
[NetworkedComponent]
[AutoGenerateComponentState]
[Access(typeof(CP14SharedVampireSystem))]
public sealed partial class CP14VampireAltarComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Multiplier = 2f;
}
