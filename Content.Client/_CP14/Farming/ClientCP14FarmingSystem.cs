using Content.Shared._CP14.Farming;
using Content.Shared.Rounding;
using Robust.Client.GameObjects;

namespace Content.Client._CP14.Farming;

public sealed partial class ClientCP14FarmingSystem : CP14SharedFarmingSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14PlantVisualizerComponent, ComponentInit>(OnPlantVisualInit);
        SubscribeLocalEvent<CP14PlantVisualizerComponent, AppearanceChangeEvent>(OnPlantAppearanceChange);
    }

    private void OnPlantAppearanceChange(Entity<CP14PlantVisualizerComponent> visuals, ref AppearanceChangeEvent args)
    {
        UpdateVisuals(visuals);
    }

    private void OnPlantVisualInit(Entity<CP14PlantVisualizerComponent> visuals, ref ComponentInit args)
    {
        UpdateVisuals(visuals);
    }

    private void UpdateVisuals(Entity<CP14PlantVisualizerComponent> visuals)
    {
        if (!TryComp<SpriteComponent>(visuals, out var sprite))
            return;

        if (!TryComp<CP14PlantComponent>(visuals, out var plant))
            return;

        var growthState = ContentHelpers.RoundToNearestLevels(plant.GrowthLevel, 1, visuals.Comp.GrowthSteps);
        if (growthState == 0)
            growthState++;

        if (sprite.LayerMapTryGet(PlantVisualLayers.Base, out _))
            sprite.LayerSetState(PlantVisualLayers.Base, $"{visuals.Comp.GrowState}{growthState}");

        if (sprite.LayerMapTryGet(PlantVisualLayers.BaseUnshaded, out _))
            sprite.LayerSetState(PlantVisualLayers.BaseUnshaded, $"{visuals.Comp.GrowUnshadedState}{growthState}");
    }
}
