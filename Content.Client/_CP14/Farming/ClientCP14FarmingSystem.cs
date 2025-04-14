using Content.Shared._CP14.Farming;
using Content.Shared._CP14.Farming.Components;
using Content.Shared.Rounding;
using Robust.Client.GameObjects;

namespace Content.Client._CP14.Farming;

public sealed class ClientCP14FarmingSystem : CP14SharedFarmingSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14PlantVisualsComponent, ComponentInit>(OnPlantVisualInit);
        SubscribeLocalEvent<CP14PlantComponent, AfterAutoHandleStateEvent>(OnAutoHandleState);
    }

    private void OnAutoHandleState(Entity<CP14PlantComponent> plant, ref AfterAutoHandleStateEvent args)
    {
        if (!TryComp<CP14PlantVisualsComponent>(plant, out var visuals))
            return;

        UpdateVisuals(new Entity<CP14PlantVisualsComponent>(plant, visuals));
    }

    private void OnPlantVisualInit(Entity<CP14PlantVisualsComponent> visuals, ref ComponentInit args)
    {
        UpdateVisuals(visuals);
    }

    private void UpdateVisuals(Entity<CP14PlantVisualsComponent> visuals)
    {
        if (!TryComp<SpriteComponent>(visuals, out var sprite))
            return;

        if (!PlantQuery.TryComp(visuals, out var plant))
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
