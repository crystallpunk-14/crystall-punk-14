using Content.Server.GameTicking.Rules;
using Content.Shared.Random;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server._CP14.StationGamePresets;

/// <summary>
/// Initializes a procedurally generated world with points of interest
/// </summary>
[RegisterComponent, Access(typeof(CP14MapSpecificRuleSystem))]
public sealed partial class CP14StationGamePresetsComponent : Component
{
    [DataField]
    public ProtoId<WeightedRandomPrototype> WeightPresets;
}
