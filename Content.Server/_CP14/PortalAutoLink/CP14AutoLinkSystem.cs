using Content.Shared.Interaction;
using Content.Shared.Teleportation.Systems;

namespace Content.Server._CP14.PortalAutoLink;

public sealed partial class CP14AutoLinkSystem : EntitySystem
{
    [Dependency] private readonly LinkedEntitySystem _link = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14AutoLinkComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<CP14AutoLinkComponent> autolink, ref MapInitEvent args)
    {
        TryAutoLink(autolink, out var otherLink);
    }
    public bool TryAutoLink(Entity<CP14AutoLinkComponent> autolink, out EntityUid? linkedEnt)
    {
        linkedEnt = null;

        var query = EntityQueryEnumerator<CP14AutoLinkComponent>();
        while (query.MoveNext(out var uid, out var otherAutolink))
        {
            if (autolink.Comp == otherAutolink)
                continue;

            if (autolink.Comp.AutoLinkKey == otherAutolink.AutoLinkKey)
            {
                if (_link.TryLink(autolink, uid, false))
                {
                    RemComp<CP14AutoLinkComponent>(uid);
                    RemComp<CP14AutoLinkComponent>(autolink);
                    return true;
                }
            }
        }
        return false;
    }
}
