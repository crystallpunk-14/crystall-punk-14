using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Butchering;

/// <summary>
/// Optional CP14-specific DoAfter event (kept for extensibility).
/// Not used directly by current implementation.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class CP14StageDoAfterEvent : SimpleDoAfterEvent
{
}
