using Content.Server._CP14.Demiplane;
using Content.Shared._CP14.Demiplane.Components;
using Robust.Shared.Map.Components;

namespace Content.Server._CP14.DemiplaneAdmin;

public sealed partial class CP14DemiplaneAdminSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14DemiplaneGenerationCatchAttemptEvent>(OnAdminDemiplaneCatch);
    }

    private void OnAdminDemiplaneCatch(CP14DemiplaneGenerationCatchAttemptEvent ev)
    {
        if (ev.Handled)
            return;

        var query = EntityQueryEnumerator<CP14DemiplaneRiftCatcherComponent, MapComponent, CP14DemiplaneComponent>();
        while (query.MoveNext(out var uid, out var catcher, out var map, out var demiplane))
        {
            ev.Demiplane = (uid, demiplane);
            ev.Handled = true;
            RemCompDeferred(uid, catcher);
            return;
        }
    }
}
