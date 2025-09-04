using Content.Shared._CP14.Fishing;
using Content.Shared._CP14.Fishing.Components;
using Content.Shared.Interaction;

namespace Content.Client._CP14.Fishing;

public sealed class CP14FishingSystem : CP14SharedFishingSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14FishingRodComponent, AfterInteractEvent>(OnInteract);
    }

    private void OnInteract(EntityUid uid, CP14FishingRodComponent component, AfterInteractEvent args)
    {
        if (args.Handled)
            return;

        if (!args.CanReach || args.Target is not { Valid: true } target)
            return;

        if (!TryComp<CP14FishingPondComponent>(args.Target, out var pond))
            return;

        if (component.FishingProcess)
            return;

        OpenFishingPopup();
    }

    private void OpenFishingPopup()
    {

    }

}
