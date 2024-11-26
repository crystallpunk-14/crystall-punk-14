using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicSpellStorage;

/// <summary>
/// A component that allows you to store spells in items
/// </summary>
[RegisterComponent, Access(typeof(CP14SpellStorageSystem))]
public sealed partial class CP14SpellStorageComponent : Component
{
    /// <summary>
    /// list of spell prototypes used for initialization.
    /// </summary>
    [DataField]
    public List<EntProtoId> Spells = new();

    /// <summary>
    /// created after the initialization of spell entities.
    /// </summary>
    [DataField]
    public List<EntityUid> SpellEntities = new();

    /// <summary>
    /// allows you to use an caster's mana to create spells.
    /// </summary>
    [DataField]
    public bool CanUseCasterMana = true;

    /// <summary>
    /// Maximum number of spells in storage.
    /// </summary>
    [DataField]
    public int MaxSpellsCount = 1;

    /// <summary>
    /// allows you to getting spells from another storage.
    /// </summary>
    [DataField]
    public bool AllowSpellsGetting = false;

    /// <summary>
    /// allows you to transfer spells to another storage. It won't work if AllowSpellsGetting is True.
    /// </summary>
    [DataField]
    public bool AllowSpellsTransfering = false;

    /// <summary>
    /// It work if AllowSpellsTransfering is True. Delete storage entity after interact.
    /// </summary>
    [DataField]
    public bool DeleteStorageAfterInteract = false;
}
