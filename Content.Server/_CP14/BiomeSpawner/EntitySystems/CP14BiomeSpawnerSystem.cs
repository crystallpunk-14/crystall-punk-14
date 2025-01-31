/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using System.Linq;
using Content.Server._CP14.BiomeSpawner.Components;
using Content.Server.Decals;
using Content.Server.Parallax;
using Content.Shared.Whitelist;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._CP14.BiomeSpawner.EntitySystems;

public sealed class CP14BiomeSpawnerSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly BiomeSystem _biome = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly SharedMapSystem _maps = default!;
    [Dependency] private readonly DecalSystem _decals = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    private int _globalSeed = 0;
    public override void Initialize()
    {
        SubscribeLocalEvent<CP14BiomeSpawnerComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<Shared.GameTicking.RoundEndMessageEvent>(OnRoundEnd);

        UpdateSeed();
    }

    private void OnRoundEnd(Shared.GameTicking.RoundEndMessageEvent ev)
    {
        UpdateSeed();
    }

    private void OnMapInit(Entity<CP14BiomeSpawnerComponent> ent, ref MapInitEvent args)
    {
        SpawnBiome(ent);
        QueueDel(ent);
    }

    private void UpdateSeed()
    {
        _globalSeed = _random.Next(int.MinValue, int.MaxValue);
    }

    private void SpawnBiome(Entity<CP14BiomeSpawnerComponent> ent)
    {
        var biome = _proto.Index(ent.Comp.Biome);
        var spawnerTransform = Transform(ent);
        if (spawnerTransform.GridUid == null)
            return;

        var gridUid = spawnerTransform.GridUid.Value;

        if (!TryComp<MapGridComponent>(gridUid, out var map))
            return;

        var vec = _transform.GetGridOrMapTilePosition(ent);

        if (!_biome.TryGetTile(vec, biome.Layers, _globalSeed, map, out var tile))
            return;

        // Set new tile
        _maps.SetTile(gridUid, map, vec, tile.Value);

        var tileCenterVec = vec + map.TileSizeHalfVector;

        // Remove old decals
        var oldDecals = _decals.GetDecalsInRange(gridUid, tileCenterVec);
        foreach (var (id, _) in oldDecals)
        {
            _decals.RemoveDecal(gridUid, id);
        }

        //Add decals
        if (_biome.TryGetDecals(vec, biome.Layers, _globalSeed, map, out var decals))
        {
            foreach (var decal in decals)
            {
                _decals.TryAddDecal(decal.ID, new EntityCoordinates(gridUid, decal.Position), out _);
            }
        }

        // Remove entities
        var oldEntities = _lookup.GetEntitiesInRange(spawnerTransform.Coordinates, 0.48f, LookupFlags.Uncontained);
        // TODO: Replace this shit with GetEntitiesInBox2
        foreach (var entToRemove in oldEntities.Concat(new[] { ent.Owner })) // Do not remove self
        {
            if (!_whitelist.IsValid(ent.Comp.DeleteBlacklist, entToRemove))
                QueueDel(entToRemove);
        }

        if (_biome.TryGetEntity(vec, biome.Layers, tile.Value, _globalSeed, map, out var entityProto))
            Spawn(entityProto, new EntityCoordinates(gridUid, tileCenterVec));
    }
}
