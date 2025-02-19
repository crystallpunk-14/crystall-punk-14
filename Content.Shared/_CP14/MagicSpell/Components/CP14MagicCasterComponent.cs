namespace Content.Shared._CP14.MagicSpell.Components;

/// <summary>
///
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedMagicSystem))]
public sealed partial class CP14MagicCasterComponent : Component
{
    [DataField]
    public List<EntityUid> CastedSpells = new();
}
