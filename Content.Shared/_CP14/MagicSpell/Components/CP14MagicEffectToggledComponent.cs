using Content.Shared.DoAfter;

namespace Content.Shared._CP14.MagicSpell.Components;

/// <summary>
///
/// </summary>
[RegisterComponent, AutoGenerateComponentPause, Access(typeof(CP14SharedMagicSystem))]
public sealed partial class CP14MagicEffectToggledComponent : Component
{
    [DataField]
    public float Cooldown = 1f;

    [DataField, AutoPausedField]
    public TimeSpan NextTick = TimeSpan.Zero;

    [DataField]
    public float Frequency = 0f;

    [DataField]
    public EntityUid? Performer;

    public DoAfterId? DoAfterId;
}
