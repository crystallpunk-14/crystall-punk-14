using Content.Server.Parallax;
using Robust.Server.GameObjects;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.BiomeSpawner;

public sealed class CP14BiomeSpawnerSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly BiomeSystem _biome = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly SharedMapSystem _maps = default!;

    private int _seed = 0;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14BiomeSpawnerComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<CP14BiomeSpawnerComponent> spawner, ref MapInitEvent args)
    {
        SpawnBiome(spawner);
        QueueDel(spawner);
    }

    private void SpawnBiome(Entity<CP14BiomeSpawnerComponent> spawner)
    {
        var biome = _proto.Index(spawner.Comp.Biome);
        var parent = _transform.GetParent(spawner);

        if (parent == null)
            return;

        if (!TryComp<MapGridComponent>(parent.Owner, out var map))
            return;

        var v2i = _transform.GetGridOrMapTilePosition(spawner);
        if (!_biome.TryGetTile(v2i, biome.Layers, _seed, map, out var tile))
            return;

        _maps.SetTile(parent.Owner, map, v2i, tile.Value);
    }
}
