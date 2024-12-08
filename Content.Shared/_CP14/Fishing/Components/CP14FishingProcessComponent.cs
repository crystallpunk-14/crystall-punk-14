using System.Numerics;
using Content.Shared._CP14.Fishing.Core;
using Content.Shared._CP14.Fishing.Prototypes;
using Content.Shared._CP14.Fishing.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Fishing.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(CP14FishingProcessSystem))]
public sealed partial class CP14FishingProcessComponent : Component
{
    /**
     * Boxes
     */

    [ViewVariables, AutoNetworkedField]
    public Player Player;

    [ViewVariables, AutoNetworkedField]
    public Fish Fish;

    /**
     * Progress
     */

    [ViewVariables, AutoNetworkedField]
    public float Progress;

    /**
     * Saved entities
     */

    [ViewVariables, AutoNetworkedField]
    public EntityUid? FishingRod;

    [ViewVariables, AutoNetworkedField]
    public EntityUid? User;

    [ViewVariables, AutoNetworkedField]
    public EntityUid? FishingPool;

    /**
     * Loot
     */

    [ViewVariables, AutoNetworkedField]
    public EntProtoId LootProtoId;

    /**
     * Style
     */

    [ViewVariables]
    public CP14FishingProcessStyleSheetPrototype StyleSheet;

    /**
     * Normalized
     */

    public Vector2 LootPositionNormalized => Vector2.UnitY * Fish.Position;
    public Vector2 PlayerPositionNormalized => Vector2.UnitY * Player.Position;
    public Vector2 PlayerHalfSizeNormalized => Vector2.UnitY * Player.HalfSize;
}
