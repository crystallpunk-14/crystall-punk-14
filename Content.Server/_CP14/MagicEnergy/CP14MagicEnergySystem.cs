using Content.Server.Cargo.Systems;
using Content.Shared._CP14.MagicEnergy;
using Content.Shared._CP14.MagicEnergy.Components;
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

        SubscribeLocalEvent<CP14MagicEnergyContainerComponent, PriceCalculationEvent>(OnMagicPrice);
    }

    private void OnMagicPrice(Entity<CP14MagicEnergyContainerComponent> ent, ref PriceCalculationEvent args)
    {
        args.Price += ent.Comp.Energy.Double() * 0.1;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        UpdateDraw(frameTime);
        UpdatePortRelay(frameTime);
    }
}
