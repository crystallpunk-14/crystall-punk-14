using Robust.Shared.Serialization;

namespace Content.Shared._CP14.MagicEnergy.Components;

/// <summary>
/// Allows you to examine how much energy is in that object
/// </summary>
[RegisterComponent, Access(typeof(SharedCP14MagicEnergyCrystalSlotSystem))]
public sealed partial class CP14MagicEnergyCrystalSlotComponent : Component
{
    [DataField(required: true)]
    public string SlotId = string.Empty;
}

[Serializable, NetSerializable]
public enum CP14MagicSlotVisuals : byte
{
    Inserted,
    Powered
}

public sealed class CP14MagicEnergyCrystalChangedEvent : EntityEventArgs
{
    public readonly bool Ejected;

    public CP14MagicEnergyCrystalChangedEvent(bool ejected)
    {
        Ejected = ejected;
    }
}
