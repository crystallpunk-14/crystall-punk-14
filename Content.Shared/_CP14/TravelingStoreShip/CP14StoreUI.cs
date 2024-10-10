using Content.Shared._CP14.Workbench;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.TravelingStoreShip;

[Serializable, NetSerializable]
public enum CP14StoreUiKey
{
    Key,
}

[Serializable, NetSerializable]
public sealed class CP14StoreUiState : BoundUserInterfaceState
{
    public readonly HashSet<CP14StoreUiProductEntry> Products;

    public CP14StoreUiState(HashSet<CP14StoreUiProductEntry> products)
    {
        Products = products;
    }
}

[Serializable, NetSerializable]
public readonly struct CP14StoreUiProductEntry : IEquatable<CP14StoreUiProductEntry>
{
    public readonly ProtoId<CP14StoreBuyPositionPrototype> ProtoId;

    public CP14StoreUiProductEntry(ProtoId<CP14StoreBuyPositionPrototype> protoId)
    {
        ProtoId = protoId;
    }

    public bool Equals(CP14StoreUiProductEntry other)
    {
        return ProtoId.Id == other.ProtoId.Id;
    }

    public override bool Equals(object? obj)
    {
        return obj is CP14StoreUiProductEntry other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ProtoId);
    }
}
