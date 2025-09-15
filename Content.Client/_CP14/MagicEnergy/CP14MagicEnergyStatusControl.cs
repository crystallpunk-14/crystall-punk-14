using Content.Client.Stylesheets;
using Content.Shared._CP14.MagicEnergy.Components;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Timing;

namespace Content.Client._CP14.MagicEnergy;

public sealed class CP14MagicEnergyStatusControl : Control
{
    private readonly Entity<CP14MagicEnergyContainerComponent> _parent;
    private readonly IEntityManager _entMan;
    private readonly RichTextLabel _label;
    private readonly ProgressBar _progress;

    public CP14MagicEnergyStatusControl(Entity<CP14MagicEnergyExaminableComponent> parent)
    {
        _entMan = IoCManager.Resolve<IEntityManager>();
        _progress = new ProgressBar
        {
            MaxValue = 1,
            Value = 0
        };
        _progress.SetHeight = 8f;
        _progress.ForegroundStyleBoxOverride = new StyleBoxFlat(Color.FromHex("#3fc488"));
        _progress.BackgroundStyleBoxOverride = new StyleBoxFlat(Color.FromHex("#0f2d42"));
        _progress.Margin = new Thickness(0, 4);
        _label = new RichTextLabel { StyleClasses = { StyleNano.StyleClassItemStatus } };

        if (!_entMan.TryGetComponent<CP14MagicEnergyContainerComponent>(parent, out var container))
            return;

        _parent = (parent.Owner, container);

        var boxContainer = new BoxContainer();

        boxContainer.Orientation = BoxContainer.LayoutOrientation.Vertical;

        boxContainer.AddChild(_label);
        boxContainer.AddChild(_progress);

        AddChild(boxContainer);
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        _progress.Value = (float)(_parent.Comp.Energy / _parent.Comp.MaxEnergy);

        var power = (int) (_parent.Comp.Energy / _parent.Comp.MaxEnergy * 100);
        _label.Text = $"{power}%";
    }
}
