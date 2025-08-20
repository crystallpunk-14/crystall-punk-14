namespace Content.Shared._CP14.Vampire.Components;

/// <summary>
/// To use it, all vampires of the clan must be nearby.
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedVampireSystem))]
public sealed partial class CP14MagicEffectAllVampireClanComponent : Component
{
    [DataField]
    public float Range = 3f;
}
