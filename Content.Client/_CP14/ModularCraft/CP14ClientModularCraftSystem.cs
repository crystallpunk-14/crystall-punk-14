using System.Linq;
using Content.Shared._CP14.ModularCraft;
using Content.Shared._CP14.ModularCraft.Components;
using Content.Shared.Hands;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations;
using Robust.Shared.Utility;

namespace Content.Client._CP14.ModularCraft;

public sealed class CP14ClientModularCraftSystem : CP14SharedModularCraftSystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IResourceCache _resCache = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14ModularCraftStartPointComponent, AfterAutoHandleStateEvent>(OnAfterHandleState);
        SubscribeLocalEvent<CP14ModularCraftStartPointComponent, GetInhandVisualsEvent>(OnGetInhandVisuals);
    }

    private void OnAfterHandleState(Entity<CP14ModularCraftStartPointComponent> start,
        ref AfterAutoHandleStateEvent args)
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
            {
                //Try get default sprite
                if (indexedPart.RsiPath is null)
                    continue;

                var state = $"icon";

                var defaultLayer = new PrototypeLayerData
                {
                    RsiPath = indexedPart.RsiPath,
                    State = state,
                };

                var keyCode = $"cp14-modular-icon-layer-{counterPart}-default";
                start.Comp.RevealedLayers.Add(keyCode);
                var index = sprite.AddLayer(defaultLayer);
                sprite.LayerMapSet(keyCode, index);
            }
            else
            {
                var counter = 0;
                foreach (var layer in indexedPart.IconSprite)
                {
                    var keyCode = $"cp14-modular-icon-layer-{counterPart}-{counter}";
                    start.Comp.RevealedLayers.Add(keyCode);
                    var index = sprite.AddLayer(layer);
                    sprite.LayerMapSet(keyCode, index);

                    counter++;
                }
            }


            counterPart++;
        }
    }

    private void OnGetInhandVisuals(Entity<CP14ModularCraftStartPointComponent> start, ref GetInhandVisualsEvent args)
    {
        var defaultKey = $"cp14-modular-inhand-layer-{args.Location.ToString().ToLowerInvariant()}";

        var counterPart = 0;
        foreach (var part in start.Comp.InstalledParts)
        {
            var indexedPart = _proto.Index(part);

            if (indexedPart.InhandVisuals is not null && indexedPart.InhandVisuals.TryGetValue(args.Location, out var layers))
            {
                var i = 0;
                foreach (var layer in layers)
                {
                    var key = $"{defaultKey}-{counterPart}-{i}";
                    args.Layers.Add((key, layer));
                    i++;
                }
            }
            else
            {
                //Try get default visuals
                if (indexedPart.RsiPath is null)
                    continue;

                var state = $"inhand-{args.Location.ToString().ToLowerInvariant()}";

                var defaultLayer = new PrototypeLayerData
                {
                    RsiPath = indexedPart.RsiPath,
                    State = state,
                };

                var key = $"{defaultKey}-{counterPart}-default";
                args.Layers.Add((key, defaultLayer));
            }

            counterPart++;
        }
    }
}
