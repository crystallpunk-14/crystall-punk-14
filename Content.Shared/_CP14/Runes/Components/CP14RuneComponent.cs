namespace Content.Shared._CP14.Runes.Components;

[RegisterComponent]
public sealed partial class CP14RuneComponent : Component
{
    /// <summary>
    /// What spell has been chosen
    /// </summary>
    [DataField]
    public CP14RuneSpellComponent? ChosenSpell;

    [DataField]
    public bool HasSpell = false;

    /// <summary>
    /// How much mana is needed to get the ability
    /// </summary>
    [DataField]
    public int ManaCost = 60;

    /// <summary>
    /// Has it been used?
    /// </summary>
    [DataField]
    public bool Used = false;
}
