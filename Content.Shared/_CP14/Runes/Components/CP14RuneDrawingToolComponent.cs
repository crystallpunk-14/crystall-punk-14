namespace Content.Shared._CP14.Runes.Components;

public sealed partial class CP14RuneDrawingToolComponent : Component
{
    /// <summary>
    /// Check if we have enough mana for drawing a rune
    /// </summary>
    [DataField]
    public bool HasEnoughMana = true;

    /// <summary>
    /// What spell has been chosen
    /// </summary>
    [DataField]
    public CP14RuneSpellComponent ChosenSpell;
}
