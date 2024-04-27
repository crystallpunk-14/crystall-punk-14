using Robust.Shared.Map;

namespace Content.Server._CP14.Magic.Spells.TargetPoint;

[Serializable, DataDefinition]
public sealed class MagicSpellTargetPointRandomMob : MagicSpellPointRandomMob
{
    public override void ApplyCoordinates(MagicSpellContext context, MapCoordinates coordinates)
    {
        context.TargetPoint = coordinates;
    }
}
