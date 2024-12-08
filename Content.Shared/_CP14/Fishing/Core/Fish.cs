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

    public float Position { get; private set; }

    private readonly Behavior _behavior;
    private readonly IRobustRandom _random;
    private readonly IGameTiming _timing;

    private float _speed;
    private TimeSpan _updateSpeedTime;

    public Fish(Behavior behavior, IRobustRandom random, IGameTiming timing)
    {
        _behavior = behavior;
        _random = random;
        _timing = timing;
    }

    public void Update(float frameTime)
    {
        // Update speed
        if (_timing.CurTime > _updateSpeedTime)
            UpdateSpeed();

        // Update position
        Position += _speed * frameTime;

        // Clamp position
        Position = Math.Clamp(Position, MinPosition, MaxPosition);
    }

    private void UpdateSpeed()
    {
        _speed = _behavior.CalculateSpeed(_random);
        _updateSpeedTime = _timing.CurTime + TimeSpan.FromSeconds(_random.NextFloat(1f / _behavior.Difficulty, 1f * _behavior.Difficulty));
    }
}
