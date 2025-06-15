using Content.Shared._CP14.MagicEnergy;
using Content.Shared._CP14.MagicEnergy.Components;
using Content.Shared.EntityEffects;
using Content.Shared.FixedPoint;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Chemistry.ReagentEffect;

[UsedImplicitly]
[DataDefinition]
public sealed partial class CP14ManaChange : EntityEffect
{
    [DataField(required: true)]
    public float ManaDelta = 1;

    [DataField]
    public bool Safe;

    [DataField]
    public bool ScaleByQuantity;

    protected override string ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        return Loc.GetString(ManaDelta >= 0 ? "cp14-reagent-effect-guidebook-mana-add" : "cp14-reagent-effect-guidebook-mana-remove",
            ("chance", Probability),
            ("amount", Math.Abs(ManaDelta)));
    }

    public override void Effect(EntityEffectBaseArgs args)
    {
        var entityManager = args.EntityManager;
        var scale = FixedPoint2.New(1);

        if (args is EntityEffectReagentArgs reagentArgs)
            scale = ScaleByQuantity ? reagentArgs.Quantity * reagentArgs.Scale : reagentArgs.Scale;

        var magicSystem = entityManager.System<SharedCP14MagicEnergySystem>();
        magicSystem.ChangeEnergy(args.TargetEntity, ManaDelta * scale, out var changed, out var overload, safe: Safe);

        scale -= FixedPoint2.Abs(changed + overload);

        if (!args.EntityManager.TryGetComponent<CP14MagicEnergyCrystalSlotComponent>(args.TargetEntity, out var crystalSlot))
            return;

        var slotSystem = entityManager.System<SharedCP14MagicEnergyCrystalSlotSystem>();
        slotSystem.TryChangeEnergy((args.TargetEntity, crystalSlot), ManaDelta * scale);
    }
}
