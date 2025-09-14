using Content.Server.Hands.Systems;
using Content.Shared._CP14.Runes;
using Content.Shared._CP14.Runes.Components;
using Content.Shared.Interaction;

namespace Content.Server._CP14.Runes;
public sealed class CP14DrawingRuneSystem : CP14SharedDrawingRuneSystem
{
    [Dependency] private readonly HandsSystem _handsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14RuneDrawingToolComponent, AfterInteractEvent>(OnInteract);
    }

    private void OnInteract(EntityUid uid, Entity<CP14RuneDrawingToolComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Handled)
            return;

        if (args.Target is not { Valid: true })
            return;

        if (!TryComp<CP14RuneDrawingToolComponent>(uid, out _))
            return;

        args.Handled = true;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
    }
}

