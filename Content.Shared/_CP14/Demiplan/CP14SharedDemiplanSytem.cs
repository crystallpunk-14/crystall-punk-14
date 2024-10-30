using Content.Shared._CP14.Demiplan.Components;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Demiplan;

public partial class CP14SharedDemiplanSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14DemiplanExitComponent, InteractHandEvent>(OnDemiplanExitInteract);
    }

    private void OnDemiplanExitInteract(Entity<CP14DemiplanExitComponent> exit, ref InteractHandEvent args)
    {
        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager,
            args.User,
            exit.Comp.DoAfter,
            new CP14DemiplanExitDoAfter(),
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
public sealed partial class CP14DemiplanExitDoAfter : SimpleDoAfterEvent
{
}
