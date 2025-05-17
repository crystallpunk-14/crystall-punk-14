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

    private const int SpeedLoopSize = 8;
    private const int DelayLoopSize = 12;

    [ViewVariables(VVAccess.ReadWrite)]
    public float Position { get; private set; }

    [ViewVariables(VVAccess.ReadWrite)]
    private readonly Behavior _behavior;

    [ViewVariables(VVAccess.ReadWrite)]
    private TimeSpan _delay;

    [ViewVariables(VVAccess.ReadWrite)]
    private float _speed;

    [ViewVariables(VVAccess.ReadWrite)]
    private float[] _speedLoop = new float[SpeedLoopSize];

    [ViewVariables(VVAccess.ReadWrite)]
    private TimeSpan[] _delayLoop = new TimeSpan[DelayLoopSize];

    [ViewVariables(VVAccess.ReadWrite)]
    private int _speedIndex;

    [ViewVariables(VVAccess.ReadWrite)]
    private int _delayIndex;

    public Fish(Behavior behavior, TimeSpan delay)
    {
        _behavior = behavior;
        _delay = delay;
    }

    public void Init(IRobustRandom random)
    {
        for (var i = 0; i < SpeedLoopSize; i++)
        {
            _speedLoop[i] = _behavior.CalculateSpeed(random);
        }

        for (var i = 0; i < DelayLoopSize; i++)
        {
            _delayLoop[i] = _behavior.CalculateDelay(random);
        }
    }

    public void Update(float frameTime)
    {
        Position += _speed * frameTime;
        Position = Math.Clamp(Position, MinPosition, MaxPosition);
    }

    public void UpdateSpeed(IGameTiming timing)
    {
        if (_delay > timing.CurTime)
            return;

        _speed = GetNextSpeed();
        _delay = GetNextDelay(timing);
    }

    private float GetNextSpeed()
    {
        return _speedLoop[_speedIndex++ % _speedLoop.Length];
    }

    private TimeSpan GetNextDelay(IGameTiming timing)
    {
        return timing.CurTime + _delayLoop[_delayIndex++ % _delayLoop.Length];
    }
}
