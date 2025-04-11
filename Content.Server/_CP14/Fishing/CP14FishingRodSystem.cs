using Content.Shared._CP14.Fishing.Components;
using Content.Shared._CP14.Fishing.Systems;
using Content.Shared.Interaction;

namespace Content.Server._CP14.Fishing;

public sealed class CP14FishingRodSystem : CP14SharedFishingRodSystem
{
    [Dependency] private readonly CP14FishingProcessSystem _fishingProcess = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14FishingPoolComponent, AfterInteractUsingEvent>(OnAfterInteractUsing);
    }

    private void OnAfterInteractUsing(Entity<CP14FishingPoolComponent> entity, ref AfterInteractUsingEvent args)
    {
        if (args.Handled || !args.CanReach)
            return;

        if (!TryComp<CP14FishingRodComponent>(args.Used, out var fishingRodComponent))
            return;

        if (fishingRodComponent.Process is not null)
            return;

        fishingRodComponent.Process = _fishingProcess.Start((args.Used, fishingRodComponent), entity, args.User);
    }
}
