using Content.Server._CP14.MagicEnergy;
using Content.Shared.EntityEffects;
using Content.Shared.FixedPoint;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Chemistry.ReagentEffect;

[UsedImplicitly]
[DataDefinition]
public sealed partial class CP14ManaChange : EntityEffect
{
    [DataField(required: true)]
    public float ManaDelta = 1;

    [DataField]
    public bool Safe = false;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        return Loc.GetString(ManaDelta >= 0 ? "cp14-reagent-effect-guidebook-mana-add" : "cp14-reagent-effect-guidebook-mana-remove",
            ("chance", Probability),
            ("amount", Math.Abs(ManaDelta)));
    }

    public override void Effect(EntityEffectBaseArgs args)
    {
        var scale = FixedPoint2.New(1);

        if (args is EntityEffectReagentArgs reagentArgs)
        {
            scale = reagentArgs.Quantity * reagentArgs.Scale;
        }

        var magicSystem = args.EntityManager.System<CP14MagicEnergySystem>();
        magicSystem.ChangeEnergy(args.TargetEntity, ManaDelta * scale, Safe);
    }
}
