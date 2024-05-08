using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Magic.Events;

[Serializable, NetSerializable]
public sealed partial class CPMagicCastDoAfterEvent : SimpleDoAfterEvent;
