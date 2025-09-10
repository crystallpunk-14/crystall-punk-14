using System.Text;
using Content.Client.Items;
using Content.Client.Stylesheets;
using Content.Shared._CP14.LockKey.Components;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Timing;

namespace Content.Client._CP14.LockKey;

public sealed class CP14ClientLockKeySystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        Subs.ItemStatus<CP14KeyComponent>(ent => new CP14KeyStatusControl(ent));
    }
}


public sealed class CP14KeyStatusControl : Control
{
    private readonly Entity<CP14KeyComponent> _parent;
    private readonly RichTextLabel _label;
    public CP14KeyStatusControl(Entity<CP14KeyComponent> parent)
    {
        _parent = parent;

        _label = new RichTextLabel { StyleClasses = { StyleNano.StyleClassItemStatus } };
        AddChild(_label);
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        if (_parent.Comp.LockShape is null)
            return;

        var sb = new StringBuilder("(");
        foreach (var item in _parent.Comp.LockShape)
        {
            sb.Append($"{item} ");
        }

        sb.Append(")");
        _label.Text = sb.ToString();
    }
}
