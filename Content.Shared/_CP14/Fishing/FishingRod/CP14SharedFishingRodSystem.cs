using Content.Shared._CP14.Fishing.FishingPool;
using Content.Shared.Hands.Components;
using Content.Shared.Interaction;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Fishing.FishingRod;

public abstract class CP14SharedFishingRodSystem : EntitySystem
{
    [Dependency] private readonly CP14FishingProcessSystem _fishingProcess = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeAllEvent<RequestFishingRodReelMessage>(OnReel);

        SubscribeLocalEvent<CP14FishingPoolComponent, AfterInteractUsingEvent>(OnAfterInteractUsing);
    }

    private void OnReel(RequestFishingRodReelMessage msg, EntitySessionEventArgs args)
    {
        var player = args.SenderSession.AttachedEntity;

        if (!TryComp<HandsComponent>(player, out var hands))
            return;

        if (!TryComp<CP14FishingRodComponent>(hands.ActiveHandEntity, out var fishingRodComponent))
            return;

        if (fishingRodComponent.Reeling == msg.Reeling)
            return;

        fishingRodComponent.Reeling = msg.Reeling;
        Dirty(hands.ActiveHandEntity.Value, fishingRodComponent);
    }

    private void OnAfterInteractUsing(Entity<CP14FishingPoolComponent> entity, ref AfterInteractUsingEvent args)
    {
        if (args.Handled || !args.CanReach)
            return;

        if (!TryComp<CP14FishingRodComponent>(args.Used, out var fishingRodComponent))
            return;

        fishingRodComponent.Process = _fishingProcess.Start("Debug", (args.Used, fishingRodComponent), entity, args.User);
    }


    [Serializable, NetSerializable]
    protected sealed class RequestFishingRodReelMessage : EntityEventArgs
    {
        public bool Reeling;

        public RequestFishingRodReelMessage(bool reeling)
        {
            Reeling = reeling;
        }
    }
}
