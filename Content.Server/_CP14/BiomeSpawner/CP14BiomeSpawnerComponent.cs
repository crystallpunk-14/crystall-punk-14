/*
 * All right reserved to CrystallPunk.
 *
 * This file is sublicensed under Custom License Agreement to Peptide (Discord) for Nuclear14 project (https://github.com/Vault-Overseers/nuclear-14) only
 *
 * See LICENSE.TXT file in the project root for full license information.
 * Copyright (c) 2024 TheShuEd (Github)
 */

using Content.Shared.Parallax.Biomes;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.BiomeSpawner;

/// <summary>
/// fills the tile in which it is located with the contents of the biome. Includes: tile, decals and entities
/// </summary>
[RegisterComponent, Access(typeof(CP14BiomeSpawnerSystem))]
public sealed partial class CP14BiomeSpawnerComponent : Component
{
    [DataField]
    public ProtoId<BiomeTemplatePrototype> Biome = "Grasslands";
}
