namespace Content.Shared._CP14.Runes.Components;

public abstract class CP14RuneRecipeSystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {

    }
}

public enum CP14RuneRecipe
{
    Growth,
    Shrink,
    NightVision,
    Berserk,
    BlindRage,
}
