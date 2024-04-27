using Robust.Shared.Map;

namespace Content.Server._CP14.Magic;

public sealed class MagicSpellContext
{
    public EntityUid Caster;

    public MapCoordinates SourcePoint;
    public MapCoordinates TargetPoint;

    public int MaxCost = 100;
    public int Cost;
}
