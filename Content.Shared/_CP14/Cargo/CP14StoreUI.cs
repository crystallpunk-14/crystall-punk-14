using Content.Shared._CP14.Workbench;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.Cargo;

[Serializable, NetSerializable]
public enum CP14StoreUiKey
{
    Key,
}

[Serializable, NetSerializable]
public sealed class CP14StoreUiState(HashSet<CP14StoreUiProductEntry> productsBuy) : BoundUserInterfaceState
{
    public readonly HashSet<CP14StoreUiProductEntry> ProductsBuy = productsBuy;
}

[Serializable, NetSerializable]
public readonly struct CP14StoreUiProductEntry(
    string protoId,
    SpriteSpecifier? icon,
    EntProtoId? entityView,
    string name,
    string desc,
    int price,
    bool special)
{
    public readonly string ProtoId = protoId;
    public readonly SpriteSpecifier? Icon = icon;
    public readonly EntProtoId? EntityView = entityView;
    public readonly string Name = name;
    public readonly string Desc = desc;
    public readonly int Price = price;
    public readonly bool Special = special;
}
