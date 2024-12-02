using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Fishing;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(CP14FishingProcessSystem))]
public sealed partial class CP14FishingProcessComponent : Component
{
    /**
     * Boxes
     */

    [ViewVariables, AutoNetworkedField]
    public Box Player = new();

    [ViewVariables, AutoNetworkedField]
    public Box Loot = new();

    /**
     * Physics
     */

    [ViewVariables, AutoNetworkedField]
    public float Size = 16f;

    /**
     * Progress
     */

    [ViewVariables, AutoNetworkedField]
    public float Progress = 0;

    /**
     * Saved entities
     */

    [ViewVariables, AutoNetworkedField]
    public EntityUid? FishingRod;

    [ViewVariables, AutoNetworkedField]
    public EntityUid? User;

    [ViewVariables, AutoNetworkedField]
    public EntityUid? FishingPool;

    /**
     * Loot
     */

    [ViewVariables, AutoNetworkedField]
    public EntProtoId LootProtoId;

    /**
     * Style
     */

    [ViewVariables]
    public CP14FishingProcessStyleSheetPrototype StyleSheet;

    /**
     * Normalized
     */

    public Vector2 LootPositionNormalized => Vector2.UnitY * Loot.Position / Size;
    public Vector2 PlayerPositionNormalized => Vector2.UnitY * Player.Position / Size;
    public Vector2 PlayerHalfSizeNormalized => Vector2.UnitY * Player.HalfSize / Size;

    /**
     * Getters
     */

    public bool Collides => Box.Collide(Player, Loot);

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

        [ViewVariables]
        public float MovementTimer;

        public Box(float size = 2.5f, float position = 0)
        {
            Size = size;
            Position = position;
        }

        public void ClampPosition(float worldSize)
        {
            Position = MathHelper.Clamp(Position, HalfSize, worldSize - HalfSize);
        }

        public void SimulateFishMovement(IRobustRandom random, float frameTime, float speed, float amplitude, float chaosFactor, float worldSize)
        {
            MovementTimer += frameTime;
            Velocity = (float)(Math.Sin(MovementTimer * speed) * amplitude);

            if (random.NextDouble() < chaosFactor)
                Velocity += (float)((random.NextDouble() * 2 - 1) * speed * chaosFactor * amplitude);

            Position += Velocity * frameTime;
        }

        public static bool Collide(Box boxA, Box boxB)
        {
            var minA = boxA.Position - boxA.HalfSize;
            var maxA = boxA.Position + boxA.HalfSize;

            var minB = boxB.Position - boxB.HalfSize;
            var maxB = boxB.Position + boxB.HalfSize;

            return maxA >= minB && maxB >= minA;
        }
    }
}
