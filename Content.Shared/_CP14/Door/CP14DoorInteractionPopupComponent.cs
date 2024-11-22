using Robust.Shared.Audio;

namespace Content.Shared._CP14.Door;

[RegisterComponent, Access(typeof(CP14DoorInteractionPopupSystem))]
public sealed partial class CP14DoorInteractionPopupComponent : Component
{
    /// <summary>
    /// Time delay between interactions to avoid spam.
    /// </summary>
    [DataField("interactDelay")]
    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan InteractDelay = TimeSpan.FromSeconds(1.0);

    [DataField("interactString")]
    public string InteractString = "cp14-closed-door-interact-popup";

    [DataField("interactSound")]
    public SoundSpecifier InteractSound;

    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan LastInteractTime;

}
