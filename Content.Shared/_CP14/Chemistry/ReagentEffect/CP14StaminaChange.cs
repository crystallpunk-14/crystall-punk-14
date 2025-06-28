using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.EntityEffects;
using Content.Shared.FixedPoint;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Chemistry.ReagentEffect;

[UsedImplicitly]
[DataDefinition]
public sealed partial class CP14StaminaChange : EntityEffect
{
    [DataField(required: true)]
    public float StaminaDelta = 1;

    [DataField]
    public bool ScaleByQuantity;

    protected override string ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        return Loc.GetString(StaminaDelta >= 0 ? "cp14-reagent-effect-guidebook-stamina-add" : "cp14-reagent-effect-guidebook-stamina-remove",
            ("chance", Probability),
            ("amount", Math.Abs(StaminaDelta)));
    }

    public override void Effect(EntityEffectBaseArgs args)
    {
        var entityManager = args.EntityManager;
        var scale = FixedPoint2.New(1);

        if (args is EntityEffectReagentArgs reagentArgs)
            scale = ScaleByQuantity ? reagentArgs.Quantity * reagentArgs.Scale : reagentArgs.Scale;

        if (StaminaDelta < 0) //Damage
        {
            var staminaSys = entityManager.System<SharedStaminaSystem>();
            staminaSys.TakeStaminaDamage(args.TargetEntity, (float)(StaminaDelta * scale));
        }
        else //Restore
        {
            if (!entityManager.TryGetComponent<StaminaComponent>(args.TargetEntity, out var staminaComp))
                return;

            staminaComp.StaminaDamage = Math.Max(0, staminaComp.StaminaDamage - (float)(StaminaDelta * scale));
            entityManager.Dirty(args.TargetEntity, staminaComp);
        }
    }
}
