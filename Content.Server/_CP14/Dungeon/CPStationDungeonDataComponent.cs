
using Content.Shared.Procedural;
using Content.Shared.Procedural.Loot;
using Robust.Shared.Prototypes;

/// <summary>
///
/// </summary>
[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class CPStationDungeonDataComponent : Component
{
    [DataField]
    public float StartLootBudget = 15f;

    [DataField]
    public float StartMobBudget = 10f;

    [DataField]
    public float LootBudgetPerLevel = 2f;

    [DataField]
    public float MobBudgetPerLevel = 2f;
}
