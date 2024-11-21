using Content.Shared._CP14.MagicSpell.Components;
using Robust.Shared.Timing;

namespace Content.Shared._CP14.MagicSpell;

public abstract class CP14SharedMagicVisionSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14MagicVisionFadeComponent, ComponentStartup>(OnStartupFade);
    }

    private void OnStartupFade(Entity<CP14MagicVisionFadeComponent> ent, ref ComponentStartup args)
    {
        ent.Comp.SpawnTime = _timing.CurTime;
        ent.Comp.EndTime = _timing.CurTime + ent.Comp.VisibilityTime;
    }
}
