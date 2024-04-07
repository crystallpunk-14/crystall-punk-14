using Content.Shared.Interaction;
using Content.Shared.Teleportation.Systems;

namespace Content.Server._CP14.PortalAutoLink;

public sealed partial class CPAutoLinkSystem : EntitySystem
{
    [Dependency] private readonly LinkedEntitySystem _link = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CPAutoLinkComponent, ActivateInWorldEvent>(OnActivateInWorld);
    }

    private void OnActivateInWorld(Entity<CPAutoLinkComponent> autolink, ref ActivateInWorldEvent args)
    {
        TryAutoLink(autolink, out var otherLink);
    }

    public bool TryAutoLink(Entity<CPAutoLinkComponent> autolink, out EntityUid? linkedEnt)
    {
        linkedEnt = null;

        var query = EntityQueryEnumerator<CPAutoLinkComponent>();
        while (query.MoveNext(out var uid, out var otherAutolink))
        {
            if (autolink.Comp == otherAutolink)
                continue;

            if (autolink.Comp.AutoLinkKey == otherAutolink.AutoLinkKey)
            {
                if (_link.TryLink(autolink, uid, false))
                {
                    RemComp<CPAutoLinkComponent>(uid);
                    RemComp<CPAutoLinkComponent>(autolink);
                    return true;
                }
            }
        }
        return false;
    }
}
