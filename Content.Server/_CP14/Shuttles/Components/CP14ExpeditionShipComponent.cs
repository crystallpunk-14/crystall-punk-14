namespace Content.Server._CP14.Shuttles.Components;

/// <summary>
///
/// </summary>
[RegisterComponent, Access(typeof(CP14ExpeditionSystem)), AutoGenerateComponentPause]
public sealed partial class CP14ExpeditionShipComponent : Component
{
    [DataField]
    public EntityUid Station;
}

