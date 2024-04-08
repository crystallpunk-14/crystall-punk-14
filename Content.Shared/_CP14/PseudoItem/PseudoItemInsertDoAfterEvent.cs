using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.PseudoItem;

[Serializable, NetSerializable]
public sealed partial class PseudoItemInsertDoAfterEvent : SimpleDoAfterEvent
{
}
