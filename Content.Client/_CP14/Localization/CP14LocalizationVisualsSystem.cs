using Content.Shared.CCVar;
using Content.Shared.Localizations;
using Robust.Client.GameObjects;
using Robust.Shared.Configuration;

namespace Content.Client._CP14.Localization;

public sealed class CP14LocalizationVisualsSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
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
            if (!pDictionary.TryGetValue(_cfg.GetCVar(CCVars.Language), out var state))
                return;

            if (sprite.LayerMapTryGet(map, out _))
                sprite.LayerSetState(map, state);
        }
    }
}
