using Content.Shared._CP14.Vampire;
using Robust.Client.GameObjects;

namespace Content.Client._CP14.Vampire;

public sealed class CP14ClientVampireSystem : CP14SharedVampireSystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;
    protected override void OnVampireVisualsInit(Entity<CP14VampireVisualsComponent> vampire, ref ComponentInit args)
    {
        base.OnVampireVisualsInit(vampire, ref args);

        if (!EntityManager.TryGetComponent(vampire, out SpriteComponent? sprite))
            return;

        if (_sprite.LayerMapTryGet(vampire.Owner, vampire.Comp.FangsMap, out var fangsLayerIndex, false))
            _sprite.LayerSetVisible(vampire.Owner, fangsLayerIndex, true);

    }

    protected override void OnVampireVisualsShutdown(Entity<CP14VampireVisualsComponent> vampire, ref ComponentShutdown args)
    {
        base.OnVampireVisualsShutdown(vampire, ref args);

        if (!EntityManager.TryGetComponent(vampire, out SpriteComponent? sprite))
            return;

        if (_sprite.LayerMapTryGet(vampire.Owner, vampire.Comp.FangsMap, out var fangsLayerIndex, false))
            _sprite.LayerSetVisible(vampire.Owner, fangsLayerIndex, false);
    }
}
