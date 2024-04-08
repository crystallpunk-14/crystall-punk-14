using Content.Shared.Parallax.Biomes;
using Content.Shared.Procedural;
using Content.Shared.Procedural.Loot;
using Content.Shared.Salvage.Expeditions;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.Dungeon;

/// <summary>
/// Spawned inside of a salvage mission.
/// </summary>
[Prototype("dungeonLevel")]
public sealed partial class CPDungeonLevelPrototype : IPrototype
{
    [IdDataField] public string ID { get; } = default!;

    [DataField]
    public IDungeonLevel Level = default!;
}

[ImplicitDataDefinitionForInheritors]
public partial interface IDungeonLevel
{
}

public sealed partial class RandomDungeonLevel : IDungeonLevel
{
    [ViewVariables(VVAccess.ReadWrite)]
    public int Seed = -1;

    [DataField]
    public float LootBudgetPerDepth = 1f;

    [DataField]
    public float MobBudgetPerDepth = 1f;

    [DataField(required: true)]
    public ProtoId<DungeonConfigPrototype> DungeonConfig;

    [DataField(required: true)]
    public ProtoId<SalvageFactionPrototype> MobFaction;

    [DataField(required: true)]
    public ProtoId<SalvageLootPrototype> LootPrototype;
}

public sealed partial class MappingGridDungeonLevel : IDungeonLevel
{
    [DataField(required: true)]
    public ResPath Path = new("/Maps/Shuttles/cargo.yml");
}
