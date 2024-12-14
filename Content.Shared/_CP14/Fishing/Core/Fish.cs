using Content.Shared._CP14.Fishing.Core.Behaviors;
using Robust.Shared.Random;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Shared._CP14.Fishing.Core;

[Serializable, NetSerializable]
public sealed class Fish
{
    private const float MaxPosition = 1f;
    private const float MinPosition = 0f;

    [ViewVariables(VVAccess.ReadWrite)]
    public float Position { get; private set; }

    [ViewVariables(VVAccess.ReadWrite)]
    private readonly Behavior _behavior;

    [ViewVariables(VVAccess.ReadWrite)]
    private float _speed;

    [ViewVariables(VVAccess.ReadWrite)]
    private TimeSpan _updateSpeedTime;

    public Fish(Behavior behavior, TimeSpan updateSpeedTime)
    {
        _behavior = behavior;
        _updateSpeedTime = updateSpeedTime;
    }

    public void Update(float frameTime)
    {
        // Update position
        Position += _speed * frameTime;

        // Clamp position
        Position = Math.Clamp(Position, MinPosition, MaxPosition);
    }

    public void UpdateSpeed(IRobustRandom random, IGameTiming timing)
    {
        if (_updateSpeedTime > timing.CurTime)
            return;

        _speed = _behavior.CalculateSpeed(random);
        _updateSpeedTime = timing.CurTime + TimeSpan.FromSeconds(random.NextFloat(1.5f - 1f / _behavior.Difficulty, 2.5f - 1f / _behavior.Difficulty));
    }
}
