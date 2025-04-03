using Content.Shared.EntityList;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Farming.Components;

/// <summary>
/// Means that the plant can be harvested.
/// </summary>
[RegisterComponent]
public sealed partial class CP14PlantGatherableComponent : Component
{
    /// <summary>
    ///     Whitelist for specifying the kind of tools can be used on a resource
    ///     Supports multiple tags.
    /// </summary>
    [DataField(required: true)]
    public EntityWhitelist ToolWhitelist = new();

    /// <summary>
    ///     YAML example below
    ///     (Tag1, Tag2, LootTableID1, LootTableID2 are placeholders for example)
    ///     --------------------
    ///     toolWhitelist:
    ///       tags:
    ///        - Tag1
    ///        - Tag2
    ///     loot:
    ///       Tag1: LootTableID1
    ///       Tag2: LootTableID2
    /// </summary>
    [DataField]
    public Dictionary<string, ProtoId<EntityLootTablePrototype>>? Loot = new();

    /// <summary>
    /// Random shift of the appearing entity during gathering
    /// </summary>
    [DataField]
    public float GatherOffset = 0.3f;

    [DataField]
    public TimeSpan GatherDelay = TimeSpan.FromSeconds(1f);

    /// <summary>
    /// After harvesting, should the plant be completely removed?
    /// </summary>
    [DataField]
    public bool DeleteAfterHarvest;

    /// <summary>
    /// After harvest, the growth level of the plant will be reduced by the specified value
    /// </summary>
    [DataField]
    public float GrowthCostHarvest = 0.4f;

    /// <summary>
    /// What level of growth does a plant need to have before it can be harvested?
    /// </summary>
    [DataField]
    public float GrowthLevelToHarvest = 0.9f;

    /// <summary>
    /// Sound to play when gathering
    /// </summary>
    [DataField]
    public SoundSpecifier GatherSound = new SoundCollectionSpecifier("CP14GrassGathering");
}
