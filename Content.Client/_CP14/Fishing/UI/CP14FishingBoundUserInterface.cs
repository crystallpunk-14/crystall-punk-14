using Content.Shared._CP14.Fishing.Components;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._CP14.Fishing.UI;

[UsedImplicitly]
public sealed class CP14FishingBoundUserInterface : BoundUserInterface
{
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

        _fishingWindow = this.CreateWindow<CP14FishingWindow>();
        _fishingWindow.InitVisuals(rodComponent);
    }

    public override void Update()
    {
        base.Update();

        if (_fishingWindow is null || !EntMan.TryGetComponent<CP14FishingRodComponent>(Owner, out var rodComponent))
            return;

        var fish = rodComponent.CaughtFish;

        if (fish is null)
            return;

        if (!EntMan.TryGetComponent<CP14FishComponent>(fish, out var fishComponent))
            return;

        var backgroundContainer = _fishingWindow.GetChild(0);

        var progressbar = backgroundContainer.GetChild(0);
        var floatContainer = backgroundContainer.GetChild(1);
        var fishContainer = backgroundContainer.GetChild(2);

        floatContainer.Margin = new Thickness(floatContainer.Margin.Left,
            0,
            floatContainer.Margin.Right + rodComponent.FloatPosition,
            0);

        fishContainer.Margin = new Thickness(fishContainer.Margin.Left,
            0,
            fishContainer.Margin.Right + fishComponent.FishPosAndDestination.X,
            0);
    }
}
