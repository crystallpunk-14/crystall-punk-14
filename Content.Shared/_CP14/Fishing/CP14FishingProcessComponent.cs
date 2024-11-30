using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Fishing;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(CP14FishingProcessSystem))]
public sealed partial class CP14FishingProcessComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public Box Player = new();

    [ViewVariables, AutoNetworkedField]
    public Box Loot = new();

    [ViewVariables, AutoNetworkedField]
    public float Size = 16f;

    [ViewVariables, AutoNetworkedField]
    public float Gravity = 1f;

    [ViewVariables, AutoNetworkedField]
    public EntityUid? User;

    [ViewVariables, AutoNetworkedField]
    public EntityUid? FishingRod;

    [ViewVariables, AutoNetworkedField]
    public EntityUid? FishingPool;

    [ViewVariables, AutoNetworkedField]
    public EntProtoId LootProtoId;

    [ViewVariables]
    public CP14FishingProcessStyleSheetPrototype StyleSheet;

    public Vector2 PlayerPositionNormalized => Vector2.UnitY * Player.Position / Size;
    public Vector2 PlayerHalfSizeNormalized => Vector2.UnitY * Player.HalfSize / Size;

    [Serializable, NetSerializable]
    public sealed class Box
    {
        [ViewVariables]
        public float Size;

        [ViewVariables]
        public float Position;

        [ViewVariables]
        public float Velocity;

        [ViewVariables]
        public float HalfSize => Size / 2f;

        public Box(float size = 2.5f, float position = 0)
        {
            Size = size;
            Position = position;
        }

        public void Clamp(float worldSize)
        {
            Position = MathHelper.Clamp(Position, HalfSize, worldSize - HalfSize);
        }
    }
}
