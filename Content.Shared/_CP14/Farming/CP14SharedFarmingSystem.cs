using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Farming;

public abstract partial class CP14SharedFarmingSystem : EntitySystem
{
    public void AffectEnergy(Entity<CP14PlantComponent> ent, float energyDelta)
    {
        ent.Comp.Energy = MathHelper.Clamp(ent.Comp.Energy + energyDelta, 0, ent.Comp.EnergyMax);
    }
    public void AffectResource(Entity<CP14PlantComponent> ent, float resourceDelta)
    {
        ent.Comp.Resource = MathHelper.Clamp(ent.Comp.Resource + resourceDelta, 0, ent.Comp.ResourceMax);
    }

    public void AffectHealth(Entity<CP14PlantComponent> ent, float healthDelta)
    {
        ent.Comp.Health = MathHelper.Clamp(ent.Comp.Health + healthDelta, 0, ent.Comp.HealthMax);
    }

    public void AffectGrowth(Entity<CP14PlantComponent> ent, float growthDelta)
    {
        ent.Comp.GrowthLevel = MathHelper.Clamp01(ent.Comp.GrowthLevel + growthDelta);
        Dirty(ent);
    }


    [Serializable, NetSerializable]
    public sealed partial class PlantSeedDoAfterEvent : SimpleDoAfterEvent
    {
    }
}
