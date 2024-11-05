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
public sealed class CP14StoreUiState : BoundUserInterfaceState
{
    public readonly HashSet<CP14StoreUiProductEntry> ProductsBuy;
    public readonly HashSet<CP14StoreUiProductEntry> ProductsSell;

    public bool OnStation;
    public readonly TimeSpan NextTravelTime;

    public CP14StoreUiState(HashSet<CP14StoreUiProductEntry> productsBuy, HashSet<CP14StoreUiProductEntry> productsSell, bool onStation, TimeSpan time)
    {
        ProductsBuy = productsBuy;
        ProductsSell = productsSell;
        OnStation = onStation;
        NextTravelTime = time;
    }
}

[Serializable, NetSerializable]
public readonly struct CP14StoreUiProductEntry : IEquatable<CP14StoreUiProductEntry>
{
    public readonly string ProtoId;
    public readonly SpriteSpecifier Icon;
    public readonly string Name;
    public readonly string Desc;
    public readonly int Price;

    public CP14StoreUiProductEntry(string protoId, SpriteSpecifier icon, string name, string desc, int price)
    {
        ProtoId = protoId;
        Icon = icon;
        Name = name;
        Desc = desc;
        Price = price;
    }

    public bool Equals(CP14StoreUiProductEntry other)
    {
        return ProtoId == other.ProtoId;
    }

    public override bool Equals(object? obj)
    {
        return obj is CP14StoreUiProductEntry other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ProtoId, Icon, Name, Desc, Price);
    }
}
