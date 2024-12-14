using Content.Shared._CP14.Fishing.Components;
using Content.Shared.Hands;
using Content.Shared.Hands.Components;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Fishing.Systems;

public abstract class CP14SharedFishingRodSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeAllEvent<RequestFishingRodReelMessage>(OnReel);
        SubscribeLocalEvent<CP14FishingRodComponent, HandDeselectedEvent>(OnDeselected);
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

    private void OnDeselected(Entity<CP14FishingRodComponent> entity, ref HandDeselectedEvent args)
    {
        entity.Comp.Reeling = false;
        Dirty(entity);
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
