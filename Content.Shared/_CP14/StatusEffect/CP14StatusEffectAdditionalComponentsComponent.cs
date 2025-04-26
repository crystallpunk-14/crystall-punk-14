using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.StatusEffect;

/// <summary>
/// A component for a status effect entity that adds and removes components on the affected entity, at the start and end of the status effect.
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(CP14SharedStatusEffectSystem))]
public sealed partial class CP14StatusEffectAdditionalComponentsComponent : Component
{
    /// <summary>
    /// Components that will be added to the status effect entity.
    /// </summary>
    /// <remarks>
    /// Important: Only use components that you are sure cannot be added or removed by other systems! At least check that it won't break anything.
    /// </remarks>
    [DataField(required: true)]
    public ComponentRegistry Components = new();

    /// <summary>
    /// Should we override the components of the same type that already exist on the target entity?
    /// </summary>
    [DataField]
    public bool Overridde = true;
}
