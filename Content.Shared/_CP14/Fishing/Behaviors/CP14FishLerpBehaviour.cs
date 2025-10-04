using System.Numerics;
using Robust.Shared.Random;

namespace Content.Shared._CP14.Fishing.Behaviors;

public sealed partial class CP14FishLerpBehaviour : CP14FishBaseBehavior
{
    public override Vector2 TryCalculatePosition(IRobustRandom random, Vector2 fishCords)
    {
        var speed = Speed + Speed * 0.2f * random.NextFloat(-1, 1);
        var nextPos = float.Lerp(fishCords.X, fishCords.Y, speed);

        return new Vector2(nextPos, fishCords.Y);
    }
}
