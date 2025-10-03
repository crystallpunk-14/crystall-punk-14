namespace Content.Shared._CP14.Runes;

public abstract class CP14RuneRecipeSystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        throw new NotImplementedException();
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
