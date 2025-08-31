using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Butchering;

/// <summary>
/// (Optional) CP14-specific DoAfter event type if you ever need it.
/// Not used by the current implementation.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class CP14StageDoAfterEvent : SimpleDoAfterEvent
{
}
