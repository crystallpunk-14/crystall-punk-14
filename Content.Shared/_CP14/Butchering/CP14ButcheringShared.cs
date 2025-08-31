using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Butchering;

/// <summary>
/// DoAfter event for staged butchering.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class CP14StageDoAfterEvent : SimpleDoAfterEvent
{
}
