using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.EntityEffects;
using Content.Shared.FixedPoint;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Chemistry.ReagentEffect;

[UsedImplicitly]
[DataDefinition]
public sealed partial class CP14InverseEffect : EntityEffect
{
    [DataField]
    public Dictionary<ProtoId<ReagentPrototype>, ProtoId<ReagentPrototype>> Inversion = new();

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        return Loc.GetString("cp14-reagent-effect-guidebook-inverse-effect", ("chance", Probability));
    }

    public override void Effect(EntityEffectBaseArgs args)
    {
        if (args is EntityEffectReagentArgs reagentArgs)
        {
            if (reagentArgs.Source is null)
                return;

            if (reagentArgs.SolutionEntity is null)
                return;

            var solutionContainer = args.EntityManager.System<SharedSolutionContainerSystem>();

            Dictionary<ReagentId, FixedPoint2> taskList = new();

            foreach (var reagent in reagentArgs.Source.Contents)
            {
                if (Inversion.ContainsKey(reagent.Reagent.Prototype))
                    taskList.Add(reagent.Reagent, reagent.Quantity);
            }

            foreach (var task in taskList)
            {
                solutionContainer.RemoveReagent(reagentArgs.SolutionEntity.Value, task.Key, task.Value);
                solutionContainer.TryAddReagent(reagentArgs.SolutionEntity.Value, Inversion[task.Key.Prototype].Id, task.Value);
            }
            return;
        }

        // TODO: Someone needs to figure out how to do this for non-reagent effects.
        throw new NotImplementedException();
    }
}
