using Robust.Shared.Prototypes;
using Content.Shared._CP14.MagicWand;

namespace Content.Shared._CP14.MagicWand.Components;

/// <summary>
/// The component will hold the runes in the wand.
/// </summary>
[RegisterComponent, Access(typeof(CP14MagicWandRunesStorageSystem))]
public sealed partial class CP14MagicWandRunesStorageComponent : Component
{
    [DataField]
    public List<EntityUid> Runes = new();

    [DataField]
    public int RunesMaxCount = 3;
}
