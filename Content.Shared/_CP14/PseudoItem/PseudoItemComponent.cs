using Content.Shared.Item;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared._CP14.PseudoItem;
/// <summary>
/// For entities that behave like an item under certain conditions,
/// but not under most conditions.
/// </summary>
[RegisterComponent]
public sealed partial class PseudoItemComponent : Component
{
    [DataField("size", customTypeSerializer: typeof(PrototypeIdSerializer<ItemSizePrototype>))]
    public string Size = "Huge";

    public bool Active = false;

    [DataField]
    public EntityUid? SleepAction;
}
