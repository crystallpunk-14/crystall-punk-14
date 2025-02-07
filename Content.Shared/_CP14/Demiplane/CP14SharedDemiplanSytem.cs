using Content.Shared._CP14.Demiplane.Components;
using Content.Shared._CP14.DemiplaneTraveling;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Robust.Shared.Player;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Demiplane;

public abstract partial  class CP14SharedDemiplaneSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14DemiplaneRiftOpenedComponent, InteractHandEvent>(OnDemiplanePasswayInteract);
    }

    private void OnDemiplanePasswayInteract(Entity<CP14DemiplaneRiftOpenedComponent> passway, ref InteractHandEvent args)
    {
        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager,
            args.User,
            passway.Comp.DoAfter,
            new CP14DemiplanPasswayUseDoAfter(),
            args.Target,
            args.Target)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            BreakOnHandChange = true,
            NeedHand = true,
            MovementThreshold = 0.2f,
        });
    }

    public virtual bool TryTeleportIntoDemiplane(Entity<CP14DemiplaneComponent> demiplane, EntityUid? entity)
    {
        return true;
    }

    public virtual bool TryTeleportOutDemiplane(Entity<CP14DemiplaneComponent> demiplane, EntityUid? entity)
    {
        return true;
    }
}

[Serializable, NetSerializable]
public sealed partial class CP14DemiplanPasswayUseDoAfter : SimpleDoAfterEvent
{
}
