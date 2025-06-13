using Robust.Shared.GameStates;

namespace Content.Shared._CP14.Execution;

/// <summary>
/// Added to entities that can be used to execute another target.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CP14ExecutionComponent : Component
{
    /// <summary>
    /// How long the execution duration lasts.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float DoAfterDuration = 5f;

    /// <summary>
    /// Arbitrarily chosen number to multiply damage by, used to deal reasonable amounts of damage to a victim of an execution.
    /// /// </summary>
    [DataField, AutoNetworkedField]
    public float DamageMultiplier = 9f;

    /// <summary>
    /// Shown to the person performing the melee execution (attacker) upon starting a melee execution.
    /// </summary>
    [DataField]
    public LocId InternalMeleeExecutionMessage = "cp14-execution-popup-melee-initial-internal";

    /// <summary>
    /// Shown to bystanders and the victim of a melee execution when a melee execution is started.
    /// </summary>
    [DataField]
    public LocId ExternalMeleeExecutionMessage = "cp14-execution-popup-melee-initial-external";

    /// <summary>
    /// Shown to the attacker upon completion of a melee execution.
    /// </summary>
    [DataField]
    public LocId CompleteInternalMeleeExecutionMessage = "cp14-execution-popup-melee-complete-internal";

    /// <summary>
    /// Shown to bystanders and the victim of a melee execution when a melee execution is completed.
    /// </summary>
    [DataField]
    public LocId CompleteExternalMeleeExecutionMessage = "cp14-execution-popup-melee-complete-external";

    /// <summary>
    /// Shown to the person performing the self execution when starting one.
    /// </summary>
    [DataField]
    public LocId InternalSelfExecutionMessage = "cp14-execution-popup-self-initial-internal";

    /// <summary>
    /// Shown to bystanders near a self execution when one is started.
    /// </summary>
    [DataField]
    public LocId ExternalSelfExecutionMessage = "cp14-execution-popup-self-initial-external";

    /// <summary>
    /// Shown to the person performing a self execution upon completion of a do-after or on use of /suicide with a weapon that has the Execution component.
    /// </summary>
    [DataField]
    public LocId CompleteInternalSelfExecutionMessage = "cp14-execution-popup-self-complete-internal";

    /// <summary>
    /// Shown to bystanders when a self execution is completed or a suicide via execution weapon happens nearby.
    /// </summary>
    [DataField]
    public LocId CompleteExternalSelfExecutionMessage = "cp14-execution-popup-self-complete-external";

    // Not networked because this is transient inside of a tick.
    /// <summary>
    /// True if it is currently executing for handlers.
    /// </summary>
    [DataField]
    public bool Executing = false;
}
