using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Farming;

public partial class CP14SharedFarmingSystem : EntitySystem
{
    [Serializable, NetSerializable]
    public sealed partial class PlantSeedDoAfterEvent : SimpleDoAfterEvent
    {
    }
    [Serializable, NetSerializable]
    public sealed partial class PlantRemoveDoAfterEvent : SimpleDoAfterEvent
    {
    }
}
