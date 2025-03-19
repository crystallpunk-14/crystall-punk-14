using Content.Shared._CP14.Vampire;
using Content.Shared.Humanoid;
using Robust.Client.GameObjects;

namespace Content.Client._CP14.Vampire;

public sealed class CP14ClientVampireVisualsSystem : CP14SharedVampireVisualsSystem
{
    protected override void OnVampireVisualsInit(Entity<CP14VampireVisualsComponent> vampire, ref ComponentInit args)
    {
        base.OnVampireVisualsInit(vampire, ref args);

        if (!EntityManager.TryGetComponent(vampire, out SpriteComponent? sprite))
            return;

        if (sprite.LayerMapTryGet(vampire.Comp.FangsMap, out var fangsLayerIndex))
            sprite.LayerSetVisible(fangsLayerIndex, true);

    }

    protected override void OnVampireVisualsShutdown(Entity<CP14VampireVisualsComponent> vampire, ref ComponentShutdown args)
    {
        base.OnVampireVisualsShutdown(vampire, ref args);

        if (!EntityManager.TryGetComponent(vampire, out SpriteComponent? sprite))
            return;

        if (sprite.LayerMapTryGet(vampire.Comp.FangsMap, out var fangsLayerIndex))
            sprite.LayerSetVisible(fangsLayerIndex, false);
    }
}
