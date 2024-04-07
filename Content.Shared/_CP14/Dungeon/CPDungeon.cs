using Content.Shared.Procedural;
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
}
