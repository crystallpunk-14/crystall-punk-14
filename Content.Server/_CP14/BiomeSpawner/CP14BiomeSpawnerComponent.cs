using Content.Server._CP14.MeleeWeapon;
using Content.Shared.Parallax.Biomes;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.BiomeSpawner;

/// <summary>
///
/// </summary>
[RegisterComponent, Access(typeof(CP14BiomeSpawnerSystem))]
public sealed partial class CP14BiomeSpawnerComponent : Component
{
    [DataField]
    public ProtoId<BiomeTemplatePrototype> Biome = "Grasslands";
}
