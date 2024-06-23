using Content.Server._CP14.Farming.Components;
using Content.Shared._CP14.Farming;
using Content.Shared.Chemistry.Components.SolutionManager;

namespace Content.Server._CP14.Farming;

public sealed partial class CP14FarmingSystem
{
    private void InitializeResources()
    {
        SubscribeLocalEvent<CP14PlantEnergyFromLightComponent, CP14PlantEnergyUpdateEvent>(OnTakeEnergyFromLight);

        SubscribeLocalEvent<CP14PlantGrowingComponent, CP14AfterPlantUpdateEvent>(OnPlantGrowing);
        SubscribeLocalEvent<CP14PlantMetabolizerComponent, CP14AfterPlantUpdateEvent>(OnPlantMetabolizing);
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

    private void OnPlantMetabolizing(Entity<CP14PlantMetabolizerComponent> ent, ref CP14AfterPlantUpdateEvent args)
    {
        if (args.Plant.SoilUid == null ||
            !TryComp<CP14SoilComponent>(args.Plant.SoilUid, out var soil) ||
            !TryComp<CP14PlantComponent>(ent, out var plant) ||
            !TryComp<SolutionContainerManagerComponent>(args.Plant.SoilUid, out var solmanager))
            return;

        var solEntity = new Entity<SolutionContainerManagerComponent?>(args.Plant.SoilUid.Value, solmanager);
        if (!_solutionContainer.TryGetSolution(solEntity, soil.Solution, out var soln, out var solution))
            return;

        var metabolizer = _proto.Index(ent.Comp.Metabolizer);

        var splitted = _solutionContainer.SplitSolution(soln.Value, ent.Comp.SolutionPerUpdate);
        foreach (var reagent in splitted)
        {
            if (!metabolizer.Metabolization.ContainsKey(reagent.Reagent.ToString()))
                continue;

            foreach (var effect in metabolizer.Metabolization[reagent.Reagent.ToString()])
            {
                effect.Effect(new Entity<CP14PlantComponent>(ent, plant), reagent.Quantity);
            }
        }
    }
}
