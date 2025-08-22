namespace Content.Shared._CP14.UniqueLoot;

/// <summary>
/// The appearance of an entity with this component will remove all other entities with the same component and key inside, ensuring the uniqueness of the entity.
/// </summary>
[RegisterComponent]
public sealed partial class CP14SingletonComponent : Component
{
    [DataField(required: true)]
    public string Key = string.Empty;
}
