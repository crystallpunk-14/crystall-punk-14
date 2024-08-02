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
}
