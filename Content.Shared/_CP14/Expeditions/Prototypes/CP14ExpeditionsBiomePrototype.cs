using Content.Shared.Procedural;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Expeditions.Prototypes;

/// <summary>
/// Allows the expedition generator to use the specified biome along with the restrictions
/// </summary>
[Prototype("cp14ExpeditionBiome")]
public sealed partial class CP14ExpeditionsBiomePrototype : IPrototype
{
    [IdDataField] public string ID { get; } = default!;

    [DataField]
    public ProtoId<DungeonConfigPrototype> Config;

    [DataField]
    public float DifficultyCost = 0;

    //Player faced description

    //Tier restriction

    //Some abstract restriction?
}
