using Content.Server._CP14.StationDungeonMap.Components;
using Content.Server.Station.Events;
using Content.Shared._CP14.StationZLevels;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._CP14.StationDungeonMap.EntitySystems;

public sealed partial class CP14StationAbyssSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly CP14StationZLevelsSystem _zLevels = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    public override void Initialize()
    {
        base.Initialize();


        //SubscribeLocalEvent<CP14StationAbyssComponent, StationPostInitEvent>(OnStationPostInit);
    }

    //public override void Update(float frameTime)
    //{
    //    base.Update(frameTime);
//
    //    var query = new EntityQueryEnumerator<CP14StationAbyssComponent, CP14StationZLevelsComponent>();
    //    while (query.MoveNext(out var uid, out var abyss, out var zLevel))
    //    {
    //        if (_timing.CurTime < abyss.NextReloadTime && abyss.NextReloadTime != TimeSpan.Zero)
    //            continue;
//
    //        abyss.NextReloadTime = _timing.CurTime + _random.Next(abyss.MinReloadTime, abyss.MaxReloadTime);
//
    //        ReloadAbyssNow((uid, abyss));
    //    }
    //}

    //private void OnStationPostInit(Entity<CP14StationAbyssComponent> abyss, ref StationPostInitEvent args)
    //{
    //    abyss.Comp.NextReloadTime = _timing.CurTime + _random.Next(abyss.Comp.MinReloadTime, abyss.Comp.MaxReloadTime);
    //    ReloadAbyssNow(abyss);
    //}

    //public void ReloadAbyssNow(Entity<CP14StationAbyssComponent> abyss)
    //{
    //    if (!TryComp<CP14StationZLevelsComponent>(abyss, out var zLevelComp))
    //        return;
//
    //    foreach (var level in abyss.Comp.Levels)
    //    {
    //        var floor = _random.Pick(level.Value);
    //        if (!_proto.TryIndex<CP14ZLevelPrototype>(floor, out var indexedZLevel))
    //            return;
    //        var resPath = _random.Pick(indexedZLevel.Maps);
//
    //        _zLevels.SetZLevel((abyss, zLevelComp), level.Key, resPath, true);
    //    }
    //}
}
