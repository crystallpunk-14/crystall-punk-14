using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.MagicVision;

public abstract class CP14SharedMagicVisionSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;

    public readonly EntProtoId MagicTraceProto = "CP14MagicVisionMarker";

    public void SpawnMagicVision(EntityCoordinates position, SpriteSpecifier? icon, string name, string description, TimeSpan duration)
    {
        var ent = PredictedSpawnAtPosition(MagicTraceProto, position);
        var markerComp = EnsureComp<CP14MagicVisionMarkerComponent>(ent);

        markerComp.SpawnTime = _timing.CurTime;
        markerComp.EndTime = _timing.CurTime + duration;

        markerComp.Icon = icon;

        _meta.SetEntityName(ent, name);
        _meta.SetEntityDescription(ent, description);

        Dirty(ent, markerComp);
    }
}
