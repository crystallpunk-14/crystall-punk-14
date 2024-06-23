using Content.Shared._CP14.Farming.Prototypes;
using Content.Shared.FixedPoint;

namespace Content.Shared._CP14.Farming.Metabolizer;

public sealed partial class AffectPlantValues : CP14MetabolizerEffect
{
    [DataField]
    public float Energy = 0f;
    [DataField]
    public float Resource = 0f;
    [DataField]
    public float Health = 0f;
    [DataField]
    public float Growth = 0f;

    public override void Effect(Entity<CP14PlantComponent> plant, FixedPoint2 amount)
    {
        plant.Comp.Energy += Energy * (float)amount;
        plant.Comp.Resource += Resource * (float)amount;
        plant.Comp.Health += Health * (float)amount;
        plant.Comp.GrowthLevel += Growth * (float)amount;
    }
}
