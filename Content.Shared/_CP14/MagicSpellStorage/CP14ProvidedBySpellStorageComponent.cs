namespace Content.Shared._CP14.MagicSpellStorage;

/// <summary>
/// Located on the action entity, stores a reference to the object from which the action was created.
/// </summary>
[RegisterComponent, Access(typeof(CP14SpellStorageSystem))]
public sealed partial class CP14ProvidedBySpellStorageComponent : Component
{
    [DataField]
    public Entity<CP14SpellStorageComponent>? SpellStorage;
}
