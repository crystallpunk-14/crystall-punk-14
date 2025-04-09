using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicRitual;

/// <summary>
/// Ritual Behavior Controller. Creates and removes entities of magical phases
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedRitualSystem))]
public sealed partial class CP14MagicRitualComponent : Component
{
    [DataField(required: true)]
    public EntProtoId StartPhase;

    [DataField]
    public EntityUid? CurrentPhase;

    [DataField]
    public float Stability = 1f;

    [DataField]
    public float ActivationTime = 5f;

    [DataField]
    public string RitualLayerMap = "ritual";

    [DataField]
    public int MaxOrbCapacity = 3;

    [DataField]
    public float RitualRadius = 5;

    [DataField]
    public TimeSpan TriggerTime = TimeSpan.Zero;

    [DataField]
    public List<EntityUid> Orbs = new();
}
