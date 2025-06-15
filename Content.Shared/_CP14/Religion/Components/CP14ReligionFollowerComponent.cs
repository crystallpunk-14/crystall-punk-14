using Content.Shared._CP14.Religion.Prototypes;
using Content.Shared._CP14.Religion.Systems;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Religion.Components;

/// <summary>
/// Determines whether the entity is a follower of God, or may never be able to become one
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(CP14SharedReligionGodSystem))]
public sealed partial class CP14ReligionFollowerComponent : Component
{
    [DataField, AutoNetworkedField]
    public ProtoId<CP14ReligionPrototype>? Religion;

    [DataField, AutoNetworkedField]
    public HashSet<ProtoId<CP14ReligionPrototype>> RejectedReligions = new();

    [DataField]
    public EntProtoId RenounceActionProto = "CP14ActionRenounceFromGod";

    [DataField]
    public EntProtoId AppealToGofProto = "CP14ActionAppealToGod";

    [DataField]
    public EntityUid? RenounceAction;

    [DataField]
    public EntityUid? AppealAction;

    /// <summary>
    /// how much energy does the entity transfer to its god
    /// </summary>
    [DataField]
    public FixedPoint2 EnergyToGodTransfer = 0.5f;

    /// <summary>
    /// how often will the entity transfer mana to its patreon
    /// </summary>
    [DataField]
    public float ManaTransferDelay = 3f;

    /// <summary>
    /// the time of the next magic energy change
    /// </summary>
    [DataField]
    public TimeSpan NextUpdateTime { get; set; } = TimeSpan.Zero;
}
