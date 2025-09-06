using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Fishing.Components;

/// <summary>
/// Allows to fish with this item
/// </summary>
[RegisterComponent]
public sealed partial class CP14FishingRodComponent : Component
{
    public bool FishingProcess = false;

    [DataField]
    public ProtoId<CP14FishingMinigamePrototype> FishingMinigame = "Default";
}
