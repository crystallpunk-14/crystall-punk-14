using Content.Shared._CP14.MagicSpell.Components;
using Robust.Shared.Timing;

namespace Content.Shared._CP14.MagicSpell;

public abstract class CP14SharedMagicVisionSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14MagicVisionMarkerComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<CP14MagicVisionMarkerComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.SpawnTime = _timing.CurTime;
        ent.Comp.EndTime = _timing.CurTime + ent.Comp.VisibilityTime;
    }
}
