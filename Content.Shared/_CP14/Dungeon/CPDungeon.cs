using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Dungeon;

[Serializable, NetSerializable]
public sealed record CPDungeonLevelParams
{
    [ViewVariables]
    public int Depth = 0;

    [ViewVariables(VVAccess.ReadWrite)]
    public int Seed = -1;
}
