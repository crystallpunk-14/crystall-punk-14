using System.Diagnostics.CodeAnalysis;
using Content.Shared._CP14.Fishing.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Fishing.Systems;

public abstract partial class CP14SharedFishingProcessSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    protected EntityQuery<CP14FishingRodComponent> FishingRod;
    protected EntityQuery<CP14FishingPoolComponent> FishingPool;

    public override void Initialize()
    {
        base.Initialize();

        FishingRod = GetEntityQuery<CP14FishingRodComponent>();
        FishingPool = GetEntityQuery<CP14FishingPoolComponent>();
    }

    public bool TryGetByUser(EntityUid userEntityUid, [NotNullWhen(true)] out Entity<CP14FishingProcessComponent>? process)
    {
        process = null;

        var query = EntityQueryEnumerator<CP14FishingProcessComponent>();
        while (query.MoveNext(out var entityUid, out var processComponent))
        {
            if (processComponent.User != userEntityUid)
                continue;

            process = (entityUid, processComponent);
            return true;
        }

        return false;
    }
}
