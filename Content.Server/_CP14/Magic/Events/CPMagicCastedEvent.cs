using Robust.Shared.Map;

namespace Content.Server._CP14.Magic;

public sealed class CPMagicCastedEvent(EntityUid caster, EntityUid? target, EntityCoordinates coordinates) : EntityEventArgs
{
    public readonly EntityUid Caster = caster;
    public readonly EntityUid? Target = target;
    public readonly EntityCoordinates Coordinates = coordinates;
}
