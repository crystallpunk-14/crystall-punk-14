using Content.Shared.DoAfter;
using Content.Shared.Maps;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.SpawnOnTileTool;

/// <summary>
/// Allows using an item on a certain type of tile to spawn entities on it.
/// </summary>
[RegisterComponent, Access(typeof(SharedCP14SpawnOnTileToolSystem))]
public sealed partial class CP14SpawnOnTileToolComponent : Component
{
    [DataField]
    public Dictionary<ProtoId<ContentTileDefinition>, EntProtoId> Spawns = new();

    [DataField]
    public bool NeedEmptySpace = true;

    [DataField]
    public TimeSpan DoAfter = TimeSpan.FromSeconds(1f);
}

[Serializable, NetSerializable]
public sealed partial class SpawnOnTileToolAfterEvent : DoAfterEvent
{
    public override DoAfterEvent Clone() => this;
    public readonly NetCoordinates Coordinates;
    public readonly EntProtoId Spawn;

    public SpawnOnTileToolAfterEvent(IEntityManager entManager, EntityCoordinates coord, EntProtoId spawn)
    {
        Spawn = spawn;
        Coordinates = entManager.GetNetCoordinates(coord);
    }

    public SpawnOnTileToolAfterEvent(NetCoordinates coord, EntProtoId spawn)
    {
        Spawn = spawn;
        Coordinates = coord;
    }
}
