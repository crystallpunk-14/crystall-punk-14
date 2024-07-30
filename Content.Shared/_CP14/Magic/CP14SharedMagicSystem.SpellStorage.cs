using Content.Shared._CP14.MagicStorage.Components;

namespace Content.Shared._CP14.Magic;

/// <summary>
/// this part of the system is responsible for storing spells in items, and the methods players use to obtain them.
/// </summary>
public partial class CP14SharedMagicSystem
{
    private void InitializeSpellStorage()
    {
        SubscribeLocalEvent<CP14MagicSpellStorageComponent, MapInitEvent>(OnMagicStorageInit);
    }

    /// <summary>
    /// When we initialize, we create action entities, and add them to this item.
    /// </summary>
    private void OnMagicStorageInit(Entity<CP14MagicSpellStorageComponent> mStorage, ref MapInitEvent args)
    {
        foreach (var spell in mStorage.Comp.Spells)
        {
            var spellEnt = _actionContainer.AddAction(mStorage, spell);
            if (spellEnt is null)
                continue;

            mStorage.Comp.SpellEntities.Add(spellEnt.Value);
        }
    }
}
