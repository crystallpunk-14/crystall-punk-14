namespace Content.Shared._CP14.Runes.Components;

[RegisterComponent]
public sealed partial class CP14RuneAltarComponent : Component
{
    [DataField]
    public CP14RuneComponent? Rune;

    [DataField]
    public bool HasRuneOnAtlar = false!;

    [DataField]
    public CP14RuneRecipeComponent? RuneRecipe;
}
