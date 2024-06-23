using Content.Shared.DoAfter;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Shared._CP14.Farming;

public partial class CP14SharedFarmingSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;


    [Serializable, NetSerializable]
    public sealed partial class PlantSeedDoAfterEvent : SimpleDoAfterEvent
    {
    }
    [Serializable, NetSerializable]
    public sealed partial class PlantRemoveDoAfterEvent : SimpleDoAfterEvent
    {
    }
}
