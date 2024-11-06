using Content.Shared.Localizations;
using Robust.Client.GameObjects;

namespace Content.Client._CP14.Localization;

public sealed class CP14LocalizationVisualsSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14LocalizationVisualsComponent, ComponentInit>(OnCompInit);
    }

    private void OnCompInit(Entity<CP14LocalizationVisualsComponent> visuals, ref ComponentInit args)
    {
        if (!TryComp<SpriteComponent>(visuals, out var sprite))
            return;

        foreach (var (map, pDictionary) in visuals.Comp.MapStates)
        {
            if (!pDictionary.TryGetValue(ContentLocalizationManager.Culture, out var state))
                return;

            if (sprite.LayerMapTryGet(map, out _))
                sprite.LayerSetState(map, state);
        }
    }
}
