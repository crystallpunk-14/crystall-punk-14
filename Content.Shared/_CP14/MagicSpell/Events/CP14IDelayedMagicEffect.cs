namespace Content.Shared._CP14.MagicSpell.Events;

public interface ICP14DelayedMagicEffect // The speak n spell interface
{
    /// <summary>
    /// Localized string spoken by the caster when casting this spell.
    /// </summary>
    public float Delay { get; }

    public bool BreakOnMove { get; }

    public bool BreakOnDamage { get; }

    public bool Hidden{ get; }
}
