using Content.Client.Overlays;
using Robust.Client.Graphics;

namespace Content.Client._CP14.Fishing;

public sealed class CP14FishingOverlaySystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        _overlay.AddOverlay(new CP14FishingOverlay());
    }

    public override void Shutdown()
    {
        base.Shutdown();

        _overlay.RemoveOverlay<StencilOverlay>();
    }
}
