using Content.Shared.DoAfter;
using Content.Shared.Maps;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.TileEditTool;

[RegisterComponent, Access(typeof(CP14EditTileToolSystem))]
public sealed partial class CP14EditTileToolComponent : Component
{
    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(1f);

    [DataField]
    public SoundSpecifier? Sound;

    [DataField]
    public Dictionary<ProtoId<ContentTileDefinition>, ProtoId<ContentTileDefinition>> TileReplace = new();
}

[Serializable, NetSerializable]
public sealed partial class CP14TileToolReplaceDoAfter : DoAfterEvent
{
    [DataField(required:true)]
    public NetCoordinates Coordinates;

    public CP14TileToolReplaceDoAfter(NetCoordinates coordinates)
    {
        Coordinates = coordinates;
    }

    public override DoAfterEvent Clone() => this;
}
