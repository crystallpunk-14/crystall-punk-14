using Content.Shared._CP14.Fishing.Components;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Client._CP14.Fishing.UI;

[UsedImplicitly]
public sealed class CP14FishingBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    [ViewVariables]
    private CP14FishingWindow? _fishingWindow;

    public CP14FishingBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        if (!EntMan.TryGetComponent<CP14FishingRodComponent>(Owner, out var rodComponent))
            return;

        if (!_prototypeManager.Resolve(rodComponent.FishingMinigame, out var fishingMinigame))
            return;

        _fishingWindow = this.CreateWindow<CP14FishingWindow>();
        _fishingWindow.InitVisuals(fishingMinigame);
    }

    public override void Update()
    {
        base.Update();

        _fishingWindow?.UpdateDraw();
    }
}
