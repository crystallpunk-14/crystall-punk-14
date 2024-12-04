using Content.Shared._CP14.ModularCraft;
using Content.Shared._CP14.ModularCraft.Components;
using Content.Shared._CP14.MagicSpellStorage;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.ModularCraft.Modifiers;

public sealed partial class AddSpellsToSpellStorage : CP14ModularCraftModifier
{
    [DataField]
    public List<EntProtoId> Spells;

    public override void Effect(EntityManager entManager, Entity<CP14ModularCraftStartPointComponent> start, Entity<CP14ModularCraftPartComponent>? part)
    {
        if (!entManager.TryGetComponent<CP14SpellStorageComponent>(start, out var storageComp))
            return;

        var spellStorageSystem = entManager.System<CP14SpellStorageSystem>();

        foreach (var spell in Spells)
        {
            spellStorageSystem.TryAddSpellToStorage((start.Owner, storageComp), spell);
        }
    }
}
