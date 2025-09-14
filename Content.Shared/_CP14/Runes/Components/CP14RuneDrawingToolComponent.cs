namespace Content.Shared._CP14.Runes;

public sealed partial class CP14RuneDrawingToolComponent : Component
{
    /// <summary>
    /// Check if we have enough mana for drawing a rune
    /// </summary>
    [DataField]
    public bool HasEnoughMana = true;
}
