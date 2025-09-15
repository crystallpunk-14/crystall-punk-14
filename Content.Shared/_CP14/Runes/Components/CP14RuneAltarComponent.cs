namespace Content.Shared._CP14.Runes.Components;

[RegisterComponent]
public sealed partial class CP14RuneAltarComponent : Component
{
    public CP14RuneComponent? Rune;

    public bool HasRuneOnAtlar = false!;

    public CP14RuneRecipeComponent? RuneRecipe;
}
