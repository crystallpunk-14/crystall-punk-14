using Content.Shared._CP14.MagicEnergy;
using Content.Shared._CP14.MagicEnergy.Components;
using Content.Shared.Cargo;
using Robust.Shared.Timing;

namespace Content.Server._CP14.MagicEnergy;

public sealed partial class CP14MagicEnergySystem : SharedCP14MagicEnergySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly CP14MagicEnergyCrystalSlotSystem _magicSlot = default!;

    public override void Initialize()
    {
        base.Initialize();

        InitializeDraw();
        InitializePortRelay();

        SubscribeLocalEvent<CP14MagicEnergyContainerComponent, PriceCalculationEvent>(OnMagicEnergyPriceCalculation);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        UpdateDraw(frameTime);
        UpdatePortRelay(frameTime);
    }

    private void OnMagicEnergyPriceCalculation(Entity<CP14MagicEnergyContainerComponent> ent, ref PriceCalculationEvent args)
    {
        args.Price += (double)(ent.Comp.Energy * 0.1f);
    }
}
