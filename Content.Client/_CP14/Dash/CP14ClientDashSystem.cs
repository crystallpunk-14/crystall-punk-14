using Content.Shared._CP14.Dash;

namespace Content.Client._CP14.Dash;

public sealed partial class CP14DClientDashSystem : EntitySystem
{
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CP14DashComponent>();
        while (query.MoveNext(out var uid, out var dash))
        {
            SpawnAtPosition(dash.DashEffect, Transform(uid).Coordinates);
        }
    }
}
