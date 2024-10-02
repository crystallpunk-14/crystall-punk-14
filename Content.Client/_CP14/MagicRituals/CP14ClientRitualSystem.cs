using Content.Shared._CP14.MagicRitual;
using Robust.Client.GameObjects;

namespace Content.Server._CP14.MagicRituals;

public partial class CP14ClientRitualSystem : CP14SharedRitualSystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14MagicRitualComponent, AppearanceChangeEvent>(OnAppearanceChange);
    }

    private void OnAppearanceChange(Entity<CP14MagicRitualComponent> ent, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (!args.Sprite.LayerMapTryGet(ent.Comp.RitualLayerMap, out var ritualLayer))
            return;

        if (_appearance.TryGetData<Color>(ent, RitualVisuals.Color, out var ritualColor, args.Component))
        {
            args.Sprite.LayerSetColor(ritualLayer, ritualColor);
        }

        if (_appearance.TryGetData<bool>(ent, RitualVisuals.Enabled, out var enabled, args.Component))
        {
            args.Sprite.LayerSetVisible(ritualLayer, enabled);
        }
    }
}
