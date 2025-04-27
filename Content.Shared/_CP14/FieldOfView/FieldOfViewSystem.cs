using Content.Shared.Standing;

namespace Content.Shared._Finster.FieldOfView;

public sealed class FieldOfViewSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FieldOfViewComponent, DownedEvent>(OnDowned);
        SubscribeLocalEvent<FieldOfViewComponent, StoodEvent>(OnStooded);
    }

    private void OnDowned(Entity<FieldOfViewComponent> ent, ref DownedEvent args)
    {
        ent.Comp.Enabled = false;
        Dirty(ent, ent.Comp);
    }

    private void OnStooded(Entity<FieldOfViewComponent> ent, ref StoodEvent args)
    {
        ent.Comp.Enabled = true;
        Dirty(ent, ent.Comp);
    }
}
