using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicRunes;

[RegisterComponent, Access(typeof(CP14MagicRuneSystem))]
public sealed partial class CP14MagicRuneComponent : Component
{
    [DataField]
    public List<EntProtoId> Spells = new();

    [DataField]
    public List<EntityUid> SpellEntities = new();
}
