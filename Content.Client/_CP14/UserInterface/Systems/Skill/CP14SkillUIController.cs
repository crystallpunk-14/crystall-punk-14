using Content.Client._CP14.Skill;
using Content.Client._CP14.UserInterface.Systems.Skill.Window;
using Content.Client.Gameplay;
using Content.Client.UserInterface.Controls;
using Content.Shared._CP14.Skill.Components;
using Content.Shared._CP14.Skill.Prototypes;
using Content.Shared.Input;
using JetBrains.Annotations;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Client.Utility;
using Robust.Shared.Input.Binding;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Client.UserInterface.Systems.Character;

[UsedImplicitly]
public sealed class CP14SkillUIController : UIController, IOnStateEntered<GameplayState>, IOnStateExited<GameplayState>,
    IOnSystemChanged<CP14ClientSkillSystem>
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [UISystemDependency] private readonly CP14ClientSkillSystem _skill = default!;

    private CP14SkillWindow? _window;
    private CP14SkillPrototype? _selectedSkill;

    private IEnumerable<CP14SkillTreePrototype>? _allTrees;
    private MenuButton? SkillButton => UIManager.GetActiveUIWidgetOrNull<MenuBar.Widgets.GameTopMenuBar>()?.CP14SkillButton;

    public void OnStateEntered(GameplayState state)
    {
        DebugTools.Assert(_window == null);

        _window = UIManager.CreateWindow<CP14SkillWindow>();
        LayoutContainer.SetAnchorPreset(_window, LayoutContainer.LayoutPreset.CenterTop);

        CommandBinds.Builder
            .Bind(ContentKeyFunctions.CP14OpenSkillMenu,
                InputCmdHandler.FromDelegate(_ => ToggleWindow()))
            .Register<CP14SkillUIController>();

        _window.GraphControl.OnNodeSelected += SelectNode;
        _proto.PrototypesReloaded += _ => ReloadSkillTrees();

        ReloadSkillTrees();
    }

    public void OnStateExited(GameplayState state)
    {
        if (_window != null)
        {
            _window.GraphControl.OnNodeSelected -= SelectNode;

            _window.Dispose();
            _window = null;
        }

        CommandBinds.Unregister<CP14SkillUIController>();
    }

    public void OnSystemLoaded(CP14ClientSkillSystem system)
    {
        system.OnSkillUpdate += SkillUpdate;
        _player.LocalPlayerDetached += CharacterDetached;
    }

    public void OnSystemUnloaded(CP14ClientSkillSystem system)
    {
        system.OnSkillUpdate -= SkillUpdate;
        _player.LocalPlayerDetached -= CharacterDetached;
    }

    public void UnloadButton()
    {
        if (SkillButton is null)
            return;

        SkillButton.OnPressed -= SkillButtonPressed;
    }

    public void LoadButton()
    {
        if (SkillButton is null)
            return;

        SkillButton.OnPressed += SkillButtonPressed;

        if (_window is null)
            return;

        _window.OnClose += DeactivateButton;
        _window.OnOpen += ActivateButton;
    }

    private void DeactivateButton()
    {
        SkillButton!.Pressed = false;
    }

    private void ActivateButton()
    {
        SkillButton!.Pressed = true;
    }

    private void SelectNode(CP14SkillPrototype? skill)
    {
        if (_window is null)
            return;

        _selectedSkill = skill;

        if (skill == null)
        {
            _window.SkillName.Text = string.Empty;
            _window.SkillDescription.Text = string.Empty;
            _window.SkillView.Texture = null;
        }
        else
        {
            _window.SkillName.Text = Loc.GetString(skill.Name);
            _window.SkillDescription.Text = Loc.GetString(skill.Desc ?? string.Empty);
            _window.SkillView.Texture = skill.Icon.Frame0();
        }
    }

    private void SkillUpdate(SkillMenuData data)
    {
        if (_window is null)
            return;

        if (!EntityManager.TryGetComponent<CP14SkillStorageComponent>(data.Entity, out var storage))
            return;

        _window.GraphControl.SetPlayer((data.Entity, storage));

        if (_allTrees == null)
            return;

        _window.TreeTabsContainer.RemoveAllChildren();
        foreach (var tree in _allTrees)
        {
            var treeButton = new Button()
            {
                Access = AccessLevel.Public,
                Text = Loc.GetString(tree.Name),
                ToolTip = Loc.GetString(tree.Desc ?? string.Empty),
                TextAlign = Label.AlignMode.Center,
            };

            treeButton.OnPressed += args =>
            {
                _window.GraphControl.Tree = tree;
            };

            _window.TreeTabsContainer.AddChild(treeButton);
        }
    }

    private void ReloadSkillTrees()
    {
        _allTrees = _proto.EnumeratePrototypes<CP14SkillTreePrototype>();
    }

    private void CharacterDetached(EntityUid uid)
    {
        CloseWindow();
    }

    private void SkillButtonPressed(BaseButton.ButtonEventArgs args)
    {
        ToggleWindow();
    }

    private void CloseWindow()
    {
        _window?.Close();
    }

    private void ToggleWindow()
    {
        if (_window == null)
            return;

        if (SkillButton != null)
        {
            SkillButton.SetClickPressed(!_window.IsOpen);
        }

        if (_window.IsOpen)
        {
            CloseWindow();
        }
        else
        {
            _skill.RequestSkillData();
            _window.Open();
        }
    }
}
