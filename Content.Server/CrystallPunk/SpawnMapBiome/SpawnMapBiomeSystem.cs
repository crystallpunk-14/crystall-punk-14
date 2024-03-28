using Content.Server.Botany;
using Content.Server.Parallax;
using Content.Shared.Parallax.Biomes;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.Corvax.CrystallPunk.SpawnMapBiome;
public sealed partial class SpawnMapBiomeSystem : EntitySystem
{
    [Dependency] private readonly BiomeSystem _biome = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SpawnMapBiomeComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<SpawnMapBiomeComponent> map, ref MapInitEvent args)
    {
        if (!_mapManager.IsMap(map)) return;

        _biome.EnsurePlanet(map, _proto.Index(map.Comp.Biome));
    }
}
