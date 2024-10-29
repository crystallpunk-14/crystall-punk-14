using Content.Shared.Procedural;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Expeditions;

[Serializable, NetSerializable]
public sealed record CP14ExpeditionMissionParams
{
    [ViewVariables(VVAccess.ReadWrite)]
    public int Seed;

    public ProtoId<DungeonConfigPrototype> Config;
}
