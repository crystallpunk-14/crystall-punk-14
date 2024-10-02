using Content.Shared._CP14.MagicRitual;
using Robust.Server.GameObjects;

namespace Content.Server._CP14.MagicRituals;

public sealed partial class CP14RitualSystem
{
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    private void InitializeVisuals()
    {
        SubscribeLocalEvent<CP14MagicRitualPhaseComponent, CP14RitualPhaseBoundEvent>(OnPhaseBound);
    }

    private void OnPhaseBound(Entity<CP14MagicRitualPhaseComponent> ent, ref CP14RitualPhaseBoundEvent args)
    {
        if (!TryComp<CP14MagicRitualComponent>(args.Ritual, out var ritual))
            return;

        _pointLight.SetColor(ent, ent.Comp.PhaseColor);
        _appearance.SetData(args.Ritual, RitualVisuals.Color, ent.Comp.PhaseColor);
    }
}
