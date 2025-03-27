using System.Linq;
using System.Numerics;
using Content.Client._CP14.Skill;
using Content.Client._CP14.Skill.Ui;
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

        _window.LearnButton.OnPressed += _ => _skill.RequestLearnSkill(_player.LocalEntity, _selectedSkill);
        _window.GraphControl.OnNodeSelected += SelectNode;
        _window.GraphControl.OnOffsetChanged += offset =>
        {
            _window.ParallaxBackground.Offset = -offset * 0.25f + new Vector2(1000,1000); //hardcoding is bad
        };
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
        system.OnSkillUpdate += UpdateState;
        _player.LocalPlayerDetached += CharacterDetached;
    }

    public void OnSystemUnloaded(CP14ClientSkillSystem system)
    {
        system.OnSkillUpdate -= UpdateState;
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

        if (_player.LocalEntity == null)
            return;

        _selectedSkill = skill;

        if (skill == null)
        {
            _window.SkillName.Text = string.Empty;
            _window.SkillDescription.Text = string.Empty;
            _window.SkillView.Texture = null;
            _window.LearnButton.Disabled = true;
        }
        else
        {
            _window.SkillName.Text = _skill.GetSkillName(skill);
            _window.SkillDescription.SetMessage(GetSkillDescription(skill));
            _window.SkillView.Texture = skill.Icon.Frame0();
            _window.LearnButton.Disabled = !_skill.CanLearnSkill(_player.LocalEntity.Value, skill);
            _window.SkillCost.Text = skill.LearnCost.ToString();
        }
    }

    private FormattedMessage GetSkillDescription(CP14SkillPrototype skill)
    {
        var msg = new FormattedMessage();

        //Description
        msg.TryAddMarkup(_skill.GetSkillDescription(skill) + "\n", out _);

        return msg;
    }

    private void UpdateState(EntityUid player)
    {
        if (_window is null)
            return;

        if (!EntityManager.TryGetComponent<CP14SkillStorageComponent>(player, out var storage))
            return;

        _window.GraphControl.UpdateState((player, storage));

        // Reselect for update state
        SelectNode(_selectedSkill);

        //If tree not selected, select the first one
        if (_window.GraphControl.Tree == null && storage.Progress.Count > 0)
        {
            var firstTree = storage.Progress.First().Key;

            if (_proto.TryIndex(firstTree, out var indexedTree))
            {
                SelectTree(indexedTree, storage); // Set the first tree from the player's progress
            }
        }

        // Update the experience points for the selected tree
        var playerProgress = storage.Progress;
        if (_window.GraphControl.Tree is not null && playerProgress.TryGetValue(_window.GraphControl.Tree, out var skillpoint))
        {
            _window.ExpPointLabel.Text = skillpoint.ToString();
        }

        _window.LevelLabel.Text = $"{storage.SkillsSumExperience}/{storage.ExperienceMaxCap}";

        _window.TreeTabsContainer.RemoveAllChildren();
        foreach (var (tree, progress) in storage.Progress)
        {
            if (!_proto.TryIndex(tree, out var indexedTree))
                continue;

            var treeButton2 = new CP14SkillTreeButtonControl(indexedTree.Color, Loc.GetString(indexedTree.Name));
            treeButton2.ToolTip = Loc.GetString(indexedTree.Desc ?? string.Empty);
            treeButton2.OnPressed += () =>
            {
                SelectTree(indexedTree, storage);
            };

            _window.TreeTabsContainer.AddChild(treeButton2);
        }
    }

    private void SelectTree(CP14SkillTreePrototype tree, CP14SkillStorageComponent storage)
    {
        if (_window == null)
            return;

        _window.GraphControl.Tree = tree;
        _window.ParallaxBackground.ParallaxPrototype = tree.Parallax;
        _window.TreeName.Text = Loc.GetString(tree.Name);

        var playerProgress = storage.Progress;
        _window.ExpPointLabel.Text = playerProgress.TryGetValue(tree, out var skillpoint) ? skillpoint.ToString() : "0";
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
