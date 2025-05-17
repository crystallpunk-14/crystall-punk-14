using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Fishing.Core;

[Serializable, NetSerializable]
public sealed class Player
{
    [ViewVariables(VVAccess.ReadWrite)]
    public float Size;

    [ViewVariables(VVAccess.ReadWrite)]
    public float Position;

    [ViewVariables(VVAccess.ReadWrite)]
    public float Velocity;

    [ViewVariables]
    public float HalfSize => Size / 2f;

    public Player(float size = 0.25f)
    {
        Size = size;
        Position = HalfSize;
    }
}
