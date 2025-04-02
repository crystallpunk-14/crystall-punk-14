using Content.Shared._CP14.Farming.Components;
using Content.Shared.Chemistry.Components.SolutionManager;

namespace Content.Server._CP14.Farming;

public sealed partial class CP14FarmingSystem
{
    private void InitializeResources()
    {
        SubscribeLocalEvent<CP14PlantEnergyFromLightComponent, CP14PlantUpdateEvent>(OnTakeEnergyFromLight);
        SubscribeLocalEvent<CP14PlantMetabolizerComponent, CP14PlantUpdateEvent>(OnPlantMetabolizing);
        SubscribeLocalEvent<CP14PlantFadingComponent, CP14PlantUpdateEvent>(OnPlantFade);
        SubscribeLocalEvent<CP14PlantFadingComponent, MapInitEvent>(OnMapInit);

        SubscribeLocalEvent<CP14PlantGrowingComponent, CP14AfterPlantUpdateEvent>(OnPlantGrowing);
    }

    private void OnMapInit(Entity<CP14PlantFadingComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.BirthTime = _timing.CurTime;
    }

    private void OnPlantFade(Entity<CP14PlantFadingComponent> ent, ref CP14PlantUpdateEvent args)
    {
        var age = _timing.CurTime - ent.Comp.BirthTime;
        var realFade = ent.Comp.ResourcePerMinute * (float)age.TotalMinutes;
        if (args.Plant.Comp.Resource < realFade)
        {
            _damageable.TryChangeDamage(ent, ent.Comp.FadeDamage, true);
        }
        AffectResource(args.Plant, -realFade);
    }

    private void OnTakeEnergyFromLight(Entity<CP14PlantEnergyFromLightComponent> regeneration, ref CP14PlantUpdateEvent args)
    {
        var gainEnergy = false;
        var daylight = true;//_dayCycle.UnderSunlight(regeneration);

        if (regeneration.Comp.Daytime && daylight)
            gainEnergy = true;

        if (regeneration.Comp.Nighttime && !daylight)
            gainEnergy = true;

        if (gainEnergy)
            args.EnergyDelta += regeneration.Comp.Energy;
    }

    private void OnPlantGrowing(Entity<CP14PlantGrowingComponent> growing, ref CP14AfterPlantUpdateEvent args)
    {
        if (args.Plant.Comp.Energy < growing.Comp.EnergyCost)
            return;

        if (args.Plant.Comp.Resource < growing.Comp.ResourceCost)
            return;

        if (args.Plant.Comp.GrowthLevel >= 1)
            return;

        AffectEnergy(args.Plant, -growing.Comp.EnergyCost);
        AffectResource(args.Plant, -growing.Comp.ResourceCost);

        AffectGrowth(args.Plant, growing.Comp.GrowthPerUpdate);
    }

    private void OnPlantMetabolizing(Entity<CP14PlantMetabolizerComponent> ent, ref CP14PlantUpdateEvent args)
    {
        if (args.Plant.Comp.SoilUid == null ||
            !SoilQuery.TryComp(args.Plant.Comp.SoilUid, out var soil) ||
            !PlantQuery.TryComp(ent, out var plant) ||
            !SolutionQuery.TryComp(args.Plant.Comp.SoilUid, out var solmanager))
            return;

        var solEntity = new Entity<SolutionContainerManagerComponent?>(args.Plant.Comp.SoilUid.Value, solmanager);
        if (!_solutionContainer.TryGetSolution(solEntity, soil.Solution, out var soln, out _))
            return;

        if (!_proto.TryIndex(ent.Comp.MetabolizerId, out var metabolizer))
            return;

        var splitted = _solutionContainer.SplitSolution(soln.Value, ent.Comp.SolutionPerUpdate);
        foreach (var reagent in splitted)
        {
            if (!metabolizer.Metabolization.ContainsKey(reagent.Reagent.ToString()))
                continue;

            foreach (var effect in metabolizer.Metabolization[reagent.Reagent.ToString()])
            {
                effect.Effect((ent, plant), reagent.Quantity, EntityManager);
            }
        }
    }
}
