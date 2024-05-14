using Content.Server._CP14.StationBiome;
using Content.Server.Parallax;
using Content.Server.Station.Components;
using Content.Server.Station.Events;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.StationLoadmap;
public sealed partial class CP14StationDungeonMapSystem : EntitySystem
{
    [Dependency] private readonly BiomeSystem _biome = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly MapSystem _map = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CP14StationDungeonMapComponent, StationPostInitEvent>(OnStationPostInit);
    }

    private void OnStationPostInit(Entity<CP14StationDungeonMapComponent> map, ref StationPostInitEvent args)
    {
        if (!HasComp<StationDataComponent>(map))
            return;

        var mapUid = _map.CreateMap(out var mapId, false);
        _metaData.SetEntityName(mapUid, map.Comp.MapName);

        _biome.EnsurePlanet(mapUid, _proto.Index(map.Comp.Biome), map.Comp.Seed, mapLight: map.Comp.MapLightColor);

        EntityManager.AddComponents(mapUid, map.Comp.Components);
    }
}
