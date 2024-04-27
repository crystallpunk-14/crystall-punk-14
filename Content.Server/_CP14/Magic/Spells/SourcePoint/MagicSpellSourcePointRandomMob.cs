using Robust.Shared.Map;

namespace Content.Server._CP14.Magic.Spells.SourcePoint;

[Serializable, DataDefinition]
public sealed partial class MagicSpellSourcePointRandomMob : MagicSpellPointRandomMob
{
    public override void ApplyCoordinates(MagicSpellContext context, MapCoordinates coordinates)
    {
        context.SourcePoint = coordinates;
    }
}
