using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Chemistry.ReagentEffect;

[UsedImplicitly]
[DataDefinition]
public sealed partial class CP14InverseEffect : Shared.Chemistry.Reagent.ReagentEffect
{
    [DataField]
    public Dictionary<ProtoId<ReagentPrototype>, ProtoId<ReagentPrototype>> Inversion = new();
    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        return Loc.GetString("cp14-reagent-effect-guidebook-inverse-effect", ("chance", Probability));
    }

    public override void Effect(ReagentEffectArgs args)
    {
        if (args.Source == null)
            return;

        if (!args.EntityManager.TryGetComponent<SolutionComponent>(args.SolutionEntity, out var solutionComp))
            return;

        var solutionContainer = args.EntityManager.System<SharedSolutionContainerSystem>();

        var ent = new Entity<SolutionComponent>(args.SolutionEntity, solutionComp);

        Dictionary<ReagentId, FixedPoint2> taskList = new();

        foreach (var reagent in args.Source.Contents)
        {
            if (Inversion.ContainsKey(reagent.Reagent.Prototype))
                taskList.Add(reagent.Reagent, reagent.Quantity);
        }

        foreach (var task in taskList)
        {
            solutionContainer.RemoveReagent(ent, task.Key, task.Value);
            solutionContainer.TryAddReagent(ent, Inversion[task.Key.Prototype].Id, task.Value);
        }
    }
}
