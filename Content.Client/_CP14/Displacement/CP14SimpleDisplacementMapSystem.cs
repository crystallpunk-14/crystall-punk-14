using Content.Client.DisplacementMap;
using Content.Shared.Item;
using Robust.Client.GameObjects;

namespace Content.Client._CP14.Displacement;

public sealed class CP14SimpleDisplacementMapSystem : EntitySystem
{
    [Dependency] private readonly DisplacementMapSystem _displacement = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14SimpleDisplacementMapComponent, VisualsChangedEvent>(OnVisualChanged);
    }

    private void OnVisualChanged(Entity<CP14SimpleDisplacementMapComponent> ent, ref VisualsChangedEvent args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        foreach (var key in ent.Comp.RevealedLayers)
        {
            sprite.RemoveLayer(key);
        }
        ent.Comp.RevealedLayers.Clear();

        foreach (var (key, dData) in ent.Comp.Displacements)
        {
            if (!sprite.LayerMapTryGet(key, out var index))
                continue;

            _displacement.TryAddDisplacement(dData, sprite, index, key, ent.Comp.RevealedLayers);
        }
    }
}
