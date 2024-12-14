using Content.Shared._CP14.Fishing.Components;
using Content.Shared._CP14.Fishing.Core;
using Content.Shared._CP14.Fishing.Prototypes;

namespace Content.Shared._CP14.Fishing.Systems;

public abstract partial class CP14SharedFishingProcessSystem
{
    protected Entity<CP14FishingRodComponent> GetRod(Entity<CP14FishingProcessComponent> process)
    {
        var entityUid = process.Comp.FishingRod!.Value;
        var component = FishingRod.GetComponent(entityUid);
        return (entityUid, component);
    }

    protected Entity<CP14FishingPoolComponent> GetPool(Entity<CP14FishingProcessComponent> process)
    {
        var entityUid = process.Comp.FishingPool!.Value;
        var component = FishingPool.GetComponent(entityUid);
        return (entityUid, component);
    }

    protected CP14FishingProcessStyleSheetPrototype GetStyle(Entity<CP14FishingRodComponent> fishingRod)
    {
        if (_prototype.TryIndex(fishingRod.Comp.Style, out var style))
            return style;

        Log.Error($"Failed to retrieve fishing rod style, {fishingRod.Comp.Style} not found. Reverting to default style.");
        return _prototype.Index(CP14FishingRodComponent.DefaultStyle);
    }

    protected static bool Collide(Fish fish, Player player)
    {
        var playerMin = player.Position - player.HalfSize;
        var playerMax = player.Position + player.HalfSize;
        return fish.Position >= playerMin && fish.Position <= playerMax;
    }
}
