using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Runes.Components;

[RegisterComponent]
public abstract partial class CP14RuneRecipeComponent : Component
{
    [DataField]
    public CP14RuneRecipe RuneRecipe;

    [DataField]
    public float Accuracy = 1.0f;
}

