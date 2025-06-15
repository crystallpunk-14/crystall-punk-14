using System.Text;
using Content.Shared._CP14.Farming;
using Content.Shared._CP14.Farming.Components;
using Content.Shared.EntityEffects;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Chemistry.ReagentEffect;

[UsedImplicitly]
[DataDefinition]
public sealed partial class CP14PlantResourceModify : EntityEffect
{
    [DataField]
    public float Energy = 0f;

    [DataField]
    public float Resourse = 0f;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        var sb = new StringBuilder();
        if (Energy != 0)
            sb.Append(Loc.GetString(Energy > 0 ? "cp14-reagent-effect-guidebook-plant-add-energy" : "cp14-reagent-effect-guidebook-plant-remove-energy", ("amount", Energy), ("chance", Probability)));
        if (Resourse != 0)
            sb.Append(Loc.GetString(Resourse > 0 ? "cp14-reagent-effect-guidebook-plant-add-resource" : "cp14-reagent-effect-guidebook-plant-remove-resource", ("amount", Resourse), ("chance", Probability)));

        return sb.ToString();
    }

    public override void Effect(EntityEffectBaseArgs args)
    {
        var scale = 1f;

        if (args is EntityEffectReagentArgs reagentArgs)
        {
            scale = reagentArgs.Quantity.Float() * reagentArgs.Scale.Float();
        }

        if (!args.EntityManager.TryGetComponent<CP14PlantComponent>(args.TargetEntity, out var plantComp))
            return;

        var plantSystem = args.EntityManager.System<CP14SharedFarmingSystem>();

        plantSystem.AffectEnergy((args.TargetEntity, plantComp), Energy * scale);
        plantSystem.AffectResource((args.TargetEntity, plantComp), Resourse * scale);
    }
}
