namespace Content.Shared._CP14.MagicSpell.Events;

public interface ICP14DelayedMagicEffect
{
    public float Cooldown { get; }

    public float CastDelay { get; }

    public bool BreakOnMove { get; }

    public bool BreakOnDamage { get; }

    public bool Hidden{ get; }

    public float EntityDistance { get; }
}
