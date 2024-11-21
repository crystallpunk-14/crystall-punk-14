namespace Content.Shared._CP14.Temperature;

/// <summary>
/// Modifies the burning of this essence.
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedFireSpreadSystem))]
public sealed partial class CP14IgnitionModifierComponent : Component
{
    [DataField]
    public float IgnitionTimeModifier = 1f;

    [DataField]
    public bool HideCaution = false;
}
