using Content.Shared.DoAfter;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Decals;

[Serializable, NetSerializable]
public sealed partial class CP14DecalCleanerDoAfterEvent : DoAfterEvent
{
    public NetCoordinates ClickLocation;

    public CP14DecalCleanerDoAfterEvent(NetCoordinates click)
    {
        ClickLocation = click;
    }

    public override DoAfterEvent Clone() => this;
}
