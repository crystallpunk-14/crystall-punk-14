using Content.Shared._CP14.ModularCraft;
using Content.Shared._CP14.ModularCraft.Components;
using Robust.Client.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Client._CP14.ModularCraft;

public sealed class CP14ClientModularCraftSystem : CP14SharedModularCraftSystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14ModularCraftStartPointComponent, AfterAutoHandleStateEvent>(OnAfterHandleState);
    }

    private void OnAfterHandleState(Entity<CP14ModularCraftStartPointComponent> start, ref AfterAutoHandleStateEvent args)
    {
        if (!TryComp<SpriteComponent>(start, out var sprite))
            return;

        UpdateIcon(start, sprite);
    }

    private void UpdateIcon(Entity<CP14ModularCraftStartPointComponent> start, SpriteComponent? sprite = null)
    {
        if (!Resolve(start, ref sprite, false))
            return;

        //Remove old layers
        foreach (var key in start.Comp.RevealedLayers)
        {
            sprite.RemoveLayer(key);
        }
        start.Comp.RevealedLayers.Clear();

        //Add new layers
        var counterPart = 0;
        foreach (var part in start.Comp.InstalledParts)
        {
            var indexedPart = _proto.Index(part);

            if (indexedPart.IconSprite is null)
                continue;

            var counter = 0;
            foreach (var layer in indexedPart.IconSprite)
            {
                var keyCode = $"cp14-modular-part-layer-{counterPart}-{counter}";
                start.Comp.RevealedLayers.Add(keyCode);
                var index = sprite.AddLayer(layer);
                sprite.LayerMapSet(keyCode, index);

                counter++;
            }

            counterPart++;
        }
    }
}
