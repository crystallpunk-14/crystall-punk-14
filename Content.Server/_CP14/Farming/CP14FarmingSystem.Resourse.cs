using Content.Server._CP14.Farming.Components;
using Content.Shared._CP14.Farming;

namespace Content.Server._CP14.Farming;

public sealed partial class CP14FarmingSystem
{
    private void InitializeResources()
    {
        //Energy
        SubscribeLocalEvent<CP14PlantEnergyFromLightComponent, CP14PlantEnergyUpdateEvent>(OnTakeEnergyFromLight);
        //Resource
        //Update
        SubscribeLocalEvent<CP14PlantGrowingComponent, CP14AfterPlantUpdateEvent>(OnPlantGrowing);
    }

    private void OnTakeEnergyFromLight(Entity<CP14PlantEnergyFromLightComponent> regeneration, ref CP14PlantEnergyUpdateEvent args)
    {
        var gainEnergy = false;
        var daylight = _dayCycle.TryDaylightThere(regeneration, true);

        if (regeneration.Comp.Daytime && daylight)
            gainEnergy = true;

        if (regeneration.Comp.Nighttime && !daylight)
            gainEnergy = true;

        if (gainEnergy)
            args.Energy += regeneration.Comp.Energy;
    }

    private void OnPlantGrowing(Entity<CP14PlantGrowingComponent> growing, ref CP14AfterPlantUpdateEvent args)
    {
        if (args.Plant.Energy < growing.Comp.EnergyCost)
            return;

        if (args.Plant.Resource < growing.Comp.ResourceCost)
            return;

        args.Plant.Energy -= growing.Comp.EnergyCost;
        args.Plant.Resource -= growing.Comp.ResourceCost;

        args.Plant.GrowthLevel = MathHelper.Clamp01(args.Plant.GrowthLevel + growing.Comp.GrowthPerUpdate);
    }
}
