using Content.Shared._CP14.Fishing.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared._CP14.Fishing.Systems;

public sealed class CP14FishingPoolSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public EntProtoId GetLootPrototypeId(Entity<CP14FishingPoolComponent> pool)
    {
        var lootTable = _prototype.Index(pool.Comp.LootTable);
        var loot = _random.Pick(lootTable.Prototypes);

        return loot;
    }
}
