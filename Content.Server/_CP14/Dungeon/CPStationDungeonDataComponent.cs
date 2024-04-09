
using Content.Shared._CP14.Dungeon;
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
    public List<ProtoId<CPDungeonLayerPrototype>> AllowedLayers = new ();
}
