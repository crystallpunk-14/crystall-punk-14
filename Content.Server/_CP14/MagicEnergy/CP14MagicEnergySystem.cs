namespace Content.Server._CP14.MagicEnergy;

public sealed partial class CP14MagicEnergySystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();


    }


    public bool TryConsumeEnergy(EntityUid uid, float energy, CP14MagicEnergyContainerComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return false;

        if (energy <= 0)
            return true;

        if (component.Energy < energy)
            return false;

        ChangeEnergy(uid, component, -energy);
        return true;
    }

    public void ChangeEnergy(EntityUid uid, CP14MagicEnergyContainerComponent component, float energy)
    {
        component.Energy = Math.Clamp(component.Energy + energy, 0, component.MaxEnergy);
    }
}
