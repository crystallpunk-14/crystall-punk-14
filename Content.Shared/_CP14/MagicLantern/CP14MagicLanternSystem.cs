using Content.Shared._CP14.MagicEnergy.Components;

namespace Content.Shared._CP14.MagicLantern;

public partial class CP14MagicLanternSystem : EntitySystem
{

    [Dependency] private readonly SharedPointLightSystem _pointLight = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14MagicLanternComponent, CP14SlotCrystalPowerChangedEvent>(OnSlotPowerChanged);
    }

    private void OnSlotPowerChanged(Entity<CP14MagicLanternComponent> ent, ref CP14SlotCrystalPowerChangedEvent args)
    {
        SharedPointLightComponent? pointLight = null;
        if (_pointLight.ResolveLight(ent, ref pointLight))
        {
            _pointLight.SetEnabled(ent, args.Powered, pointLight);
        }
    }
}
