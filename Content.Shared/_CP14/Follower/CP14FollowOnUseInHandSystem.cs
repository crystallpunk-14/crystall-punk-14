using Content.Shared.Follower;
using Content.Shared.Interaction.Events;

namespace Content.Shared._CP14.Follower;

public partial class CP14FollowOnUseInHandSystem : EntitySystem
{

    [Dependency] private readonly FollowerSystem _followerSystem = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CP14FollowOnUseInHandComponent, UseInHandEvent>(OnUseInHand);
    }

    private void OnUseInHand(EntityUid uid, CP14FollowOnUseInHandComponent component, UseInHandEvent args)
    {
        _followerSystem.StartFollowingEntity(uid, args.User);
    }
}
