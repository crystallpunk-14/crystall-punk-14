using Content.Client.Clothing;
using Content.Shared._CP14.ModularCraft;
using Content.Shared._CP14.ModularCraft.Components;
using Content.Shared.Clothing;
using Content.Shared.Hands;
using Content.Shared.Inventory;
using Content.Shared.Item;
using Content.Shared.Wieldable.Components;
using Robust.Client.GameObjects;
using Robust.Client.ResourceManagement;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations;

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
        SubscribeLocalEvent<CP14ModularCraftStartPointComponent, GetEquipmentVisualsEvent>(OnGetEquipmentVisuals);
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

                var rsi = _resCache
                    .GetResource<RSIResource>(SpriteSpecifierSerializer.TextureRoot / indexedPart.RsiPath)
                    .RSI;

                if (!rsi.TryGetState(state, out _))
                    continue;

                var defaultLayer = new PrototypeLayerData
                {
                    RsiPath = indexedPart.RsiPath,
                    State = state,
                };

                var keyCode = $"cp14-modular-icon-layer-{counterPart}-default";
                start.Comp.RevealedLayers.Add(keyCode);
                var index = sprite.AddLayer(defaultLayer);
                sprite.LayerMapSet(keyCode, index);

                if (indexedPart.Color is not null)
                    sprite.LayerSetColor(keyCode, indexedPart.Color.Value);
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

        if (!TryComp<ItemComponent>(start, out var item))
            return;

        var wielded = item.HeldPrefix == "wielded"; //SHITCOOOOOOODE

        var counterPart = 0;
        foreach (var part in start.Comp.InstalledParts)
        {
            var indexedPart = _proto.Index(part);

            var targetLayers =
                wielded ? indexedPart.WieldedInhandVisuals : indexedPart.InhandVisuals;

            if (targetLayers is not null && targetLayers.TryGetValue(args.Location, out var layers))
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

                var rsi = _resCache
                    .GetResource<RSIResource>(SpriteSpecifierSerializer.TextureRoot / indexedPart.RsiPath)
                    .RSI;

                var state = $"inhand-{args.Location.ToString().ToLowerInvariant()}";

                if (wielded)
                    state = $"wielded-{state}";

                if (!rsi.TryGetState(state, out _))
                    continue;

                var defaultLayer = new PrototypeLayerData
                {
                    RsiPath = indexedPart.RsiPath,
                    State = state,
                    Color = indexedPart.Color,
                };

                var key = $"{defaultKey}-{counterPart}-default";
                args.Layers.Add((key, defaultLayer));
            }

            counterPart++;
        }
    }

    private void OnGetEquipmentVisuals(Entity<CP14ModularCraftStartPointComponent> start,
        ref GetEquipmentVisualsEvent args)
    {
        if (!TryComp(args.Equipee, out InventoryComponent? inventory))
            return;

        var defaultKey = $"cp14-modular-clothing-layer-{args.Slot}";

        var counterPart = 0;
        foreach (var part in start.Comp.InstalledParts)
        {
            var indexedPart = _proto.Index(part);

            if (indexedPart.ClothingVisuals is not null && indexedPart.ClothingVisuals.TryGetValue(args.Slot, out var layers))
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
                //Try get default sprites
                if (indexedPart.RsiPath is null)
                    continue;

                var rsi = _resCache
                    .GetResource<RSIResource>(SpriteSpecifierSerializer.TextureRoot / indexedPart.RsiPath)
                    .RSI;

                if (!ClientClothingSystem.TemporarySlotMap.TryGetValue(args.Slot, out var correctedSlot))
                    continue;

                var state = $"equipped-{correctedSlot}";

                if (!rsi.TryGetState(state, out _))
                    continue;

                var defaultLayer = new PrototypeLayerData
                {
                    RsiPath = indexedPart.RsiPath,
                    State = state,
                    Color = indexedPart.Color,
                };

                var key = $"{defaultKey}-{counterPart}-default";
                args.Layers.Add((key, defaultLayer));
            }
        }
    }
}
