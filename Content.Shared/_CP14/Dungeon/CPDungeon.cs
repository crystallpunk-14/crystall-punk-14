using Content.Shared.Parallax.Biomes;
using Content.Shared.Procedural;
using Content.Shared.Procedural.Loot;
using Content.Shared.Salvage.Expeditions;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Dungeon;

[Serializable, NetSerializable]
public sealed record CPDungeonLevelParams
{
    [ViewVariables]
    public int Depth = 0;

    [ViewVariables(VVAccess.ReadWrite)]
    public int Seed = -1;

    [DataField]
    public ProtoId<DungeonConfigPrototype> DungeonConfig;

    [DataField]
    public ProtoId<SalvageFactionPrototype> MobFaction;

    [DataField]
    public ProtoId<BiomeTemplatePrototype> BiomeTemplate;

    [DataField]
    public ProtoId<SalvageLootPrototype> LootPrototype;
}
