using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Temperature;

// Appearance Data key
[Serializable, NetSerializable]
public enum FireplaceFuelVisuals : byte
{
    Status,
}

[Serializable, NetSerializable]
public enum FireplaceFuelStatus : byte
{
    Empty,
    Medium,
    Full
}
