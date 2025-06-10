using Content.Shared.Actions;

namespace Content.Shared._CP14.MagicSpell.Events;

public interface ICP14MagicEffect
{
    public TimeSpan Cooldown { get; }
}

public sealed partial class CP14EntityWorldTargetActionEvent : WorldTargetActionEvent, ICP14MagicEffect
{
    [DataField]
    public TimeSpan Cooldown { get; private set; } = TimeSpan.FromSeconds(1f);
}

public sealed partial class CP14EntityTargetActionEvent : EntityTargetActionEvent, ICP14MagicEffect
{
    [DataField]
    public TimeSpan Cooldown { get; private set; } = TimeSpan.FromSeconds(1f);
}

public sealed partial class CP14InstantActionEvent : InstantActionEvent, ICP14MagicEffect
{
    [DataField]
    public TimeSpan Cooldown { get; private set; } = TimeSpan.FromSeconds(1f);
}

