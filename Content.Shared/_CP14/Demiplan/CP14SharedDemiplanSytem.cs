using Content.Shared._CP14.Demiplan.Components;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Demiplan;

public abstract partial  class CP14SharedDemiplanSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14DemiplanRiftOpenedComponent, InteractHandEvent>(OnDemiplanPasswayInteract);
    }

    private void OnDemiplanPasswayInteract(Entity<CP14DemiplanRiftOpenedComponent> passway, ref InteractHandEvent args)
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
}

[Serializable, NetSerializable]
public sealed partial class CP14DemiplanPasswayUseDoAfter : SimpleDoAfterEvent
{
}
