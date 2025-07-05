using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.Workbench.Requirements;

public sealed partial class SolutionResource : CP14WorkbenchCraftRequirement
{
    [DataField(required: true)]
    public ProtoId<ReagentPrototype> Reagent = default!;

    /// <summary>
    /// How much impurity from other reagents is allowed?
    /// </summary>
    [DataField(required: true)]
    public float Purity = 1f;

    [DataField(required: true)]
    public FixedPoint2 Amount = 1f;

    [DataField]
    public EntProtoId DummyEntityIcon = "CP14LiquidDropDummy";

    public override bool HideRecipe { get; set; } = false;

    public override bool CheckRequirement(EntityManager entManager,
        IPrototypeManager protoManager,
        HashSet<EntityUid> placedEntities,
        EntityUid? user)
    {
        var solutionSys = entManager.System<SharedSolutionContainerSystem>();
        foreach (var ent in placedEntities)
        {
            if (!solutionSys.TryGetDrawableSolution(ent, out var soln, out var solution))
                continue;

            var volume = solution.Volume;
            foreach (var (id, quantity) in solution.Contents)
            {
                if (id.Prototype != Reagent)
                    continue;

                if (quantity < Amount)
                    continue;

                //Purity check
                if (quantity / volume < Purity)
                    continue;

                return true;
            }
        }

        return false;
    }

    public override void PostCraft(EntityManager entManager, IPrototypeManager protoManager, HashSet<EntityUid> placedEntities, EntityUid? user)
    {
        var solutionSys = entManager.System<SharedSolutionContainerSystem>();
        foreach (var ent in placedEntities)
        {
            if (!solutionSys.TryGetDrawableSolution(ent, out var soln, out var solution))
                continue;


            var volume = solution.Volume;
            foreach (var (id, quantity) in solution.Contents)
            {
                if (id.Prototype != Reagent)
                    continue;

                if (quantity < Amount)
                    continue;

                //Purity check
                if (quantity / volume < Purity)
                    continue;

                solutionSys.RemoveEachReagent(soln.Value, Amount);
                return;
            }
        }
    }

    public override double GetPrice(EntityManager entManager, IPrototypeManager protoManager)
    {
        if (!protoManager.TryIndex(Reagent, out var indexedReagent))
            return 0;

        return indexedReagent.PricePerUnit * (double)Amount;
    }

    public override string GetRequirementTitle(IPrototypeManager protoManager)
    {
        if (!protoManager.TryIndex(Reagent, out var indexedReagent))
            return string.Empty;

        return Loc.GetString("cp14-workbench-reagent-req",
            ("reagent", indexedReagent.LocalizedName),
            ("count", Amount),
            ("purity", Purity * 100));
    }

    public override EntityPrototype? GetRequirementEntityView(IPrototypeManager protoManager)
    {
        if (!protoManager.TryIndex(DummyEntityIcon, out var indexedEnt))
            return null;
        return indexedEnt;
    }

    public override Color GetRequirementColor(IPrototypeManager protoManager)
    {
        if (!protoManager.TryIndex(Reagent, out var indexedReagent))
            return Color.White;

        return indexedReagent.SubstanceColor;
    }
}
