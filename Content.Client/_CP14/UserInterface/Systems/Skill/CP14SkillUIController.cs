using System.Linq;
using System.Numerics;
using System.Text;
using Content.Client._CP14.Skill;
using Content.Client._CP14.Skill.Ui;
using Content.Client._CP14.UserInterface.Systems.NodeTree;
using Content.Client._CP14.UserInterface.Systems.Skill.Window;
using Content.Client.Gameplay;
using Content.Client.UserInterface.Controls;
using Content.Shared._CP14.Skill.Components;
using Content.Shared._CP14.Skill.Prototypes;
using Content.Shared._CP14.Skill.Restrictions;
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

namespace Content.Client._CP14.UserInterface.Systems.Skill;

[UsedImplicitly]
public sealed class CP14SkillUIController : UIController, IOnStateEntered<GameplayState>, IOnStateExited<GameplayState>,
    IOnSystemChanged<CP14ClientSkillSystem>
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IEntityManager _entManager = default!;
    [UISystemDependency] private readonly CP14ClientSkillSystem _skill = default!;

    private CP14SkillWindow? _window;
    private EntityUid? _targetPlayer;

    private IEnumerable<CP14SkillPrototype> _allSkills = [];
    private IEnumerable<CP14SkillTreePrototype> _allTrees = [];

    private CP14SkillPrototype? _selectedSkill;
    private CP14SkillTreePrototype? _selectedSkillTree;

    private MenuButton? SkillButton => UIManager
        .GetActiveUIWidgetOrNull<Client.UserInterface.Systems.MenuBar.Widgets.GameTopMenuBar>()
        ?.CP14SkillButton;

    public void OnStateEntered(GameplayState state)
    {
        DebugTools.Assert(_window == null);

        _window = UIManager.CreateWindow<CP14SkillWindow>();
        LayoutContainer.SetAnchorPreset(_window, LayoutContainer.LayoutPreset.CenterTop);

        CommandBinds.Builder
            .Bind(ContentKeyFunctions.CP14OpenSkillMenu,
                InputCmdHandler.FromDelegate(_ => ToggleWindow()))
            .Register<CP14SkillUIController>();

        CacheSkillProto();
        _proto.PrototypesReloaded += _ => CacheSkillProto();

        _window.LearnButton.OnPressed += _ => _skill.RequestLearnSkill(_playerManager.LocalEntity, _selectedSkill);
        _window.GraphControl.OnNodeSelected += SelectNode;
        _window.GraphControl.OnOffsetChanged += offset =>
        {
            _window.ParallaxBackground.Offset = -offset * 0.25f + new Vector2(1000, 1000); //hardcoding is bad
        };
    }

    private void CacheSkillProto()
    {
        _allSkills = _proto.EnumeratePrototypes<CP14SkillPrototype>();
        _allTrees = _proto.EnumeratePrototypes<CP14SkillTreePrototype>();
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
        _playerManager.LocalPlayerDetached += CharacterDetached;
    }

    public void OnSystemUnloaded(CP14ClientSkillSystem system)
    {
        system.OnSkillUpdate -= UpdateState;
        _playerManager.LocalPlayerDetached -= CharacterDetached;
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

    private void SelectNode(CP14NodeTreeElement? node)
    {
        if (_window is null)
            return;

        if (_playerManager.LocalEntity == null)
            return;

        if (node == null)
        {
            DeselectNode();
            return;
        }

        if (!_proto.TryIndex<CP14SkillPrototype>(node.NodeKey, out var skill))
        {
            DeselectNode();
            return;
        }

        SelectNode(skill);
    }

    private void SelectNode(CP14SkillPrototype? skill)
    {
        if (_window is null)
            return;

        if (_playerManager.LocalEntity == null)
            return;

        _selectedSkill = skill;

        if (skill == null)
        {
            DeselectNode();
        }
        else
        {
            _window.SkillName.Text = _skill.GetSkillName(skill);
            _window.SkillDescription.SetMessage(GetSkillDescription(skill));
            _window.SkillView.Texture = skill.Icon.Frame0();
            _window.LearnButton.Disabled = !_skill.CanLearnSkill(_playerManager.LocalEntity.Value, skill);
            _window.SkillCost.Text = skill.LearnCost.ToString();
        }

        UpdateGraphControl();
    }

    private void DeselectNode()
    {
        if (_window is null)
            return;

        _window.SkillName.Text = string.Empty;
        _window.SkillDescription.Text = string.Empty;
        _window.SkillView.Texture = null;
        _window.LearnButton.Disabled = true;
    }

    private FormattedMessage GetSkillDescription(CP14SkillPrototype skill)
    {
        var msg = new FormattedMessage();

        if (_playerManager.LocalEntity == null)
            return msg;

        var sb = new StringBuilder();

        //Description
        sb.Append(_skill.GetSkillDescription(skill) + "\n \n");

        //Restrictions
        foreach (var req in skill.Restrictions)
        {
            var color = req.Check(_entManager, _playerManager.LocalEntity.Value) ? "green" : "red";

            sb.Append($"- [color={color}]{req.GetDescription(_entManager, _proto)}[/color]\n");
        }

        msg.TryAddMarkup(sb.ToString(), out _);

        return msg;
    }

    private void UpdateGraphControl()
    {
        if (_window is null)
            return;

        if (_selectedSkillTree == null)
            return;

        if (!EntityManager.TryGetComponent<CP14SkillStorageComponent>(_targetPlayer, out var storage))
            return;

        HashSet<CP14NodeTreeElement> nodeTreeElements = new();

        HashSet<(string, string)> nodeTreeEdges = new();

        var learned = storage.LearnedSkills;
        foreach (var skill in _allSkills)
        {
            if (skill.Tree != _selectedSkillTree)
                continue;

            foreach (var req in skill.Restrictions)
            {
                switch (req)
                {
                    case NeedPrerequisite prerequisite:
                        if (!_proto.TryIndex(prerequisite.Prerequisite, out var prerequisiteSkill))
                            continue;

                        if (prerequisiteSkill.Tree != _selectedSkillTree)
                            continue;

                        nodeTreeEdges.Add((skill.ID, prerequisiteSkill.ID));
                        break;
                }
            }

            var nodeTreeElement = new CP14NodeTreeElement(
                skill.ID,
                gained: learned.Contains(skill),
                active: _skill.CanLearnSkill(_targetPlayer.Value, skill),
                skill.SkillUiPosition * 25f,
                skill.Icon);
            nodeTreeElements.Add(nodeTreeElement);
        }

        _window.GraphControl.UpdateState(
            new CP14NodeTreeUiState(
                nodes: nodeTreeElements,
                edges: nodeTreeEdges,
                frameIcon: _selectedSkillTree.FrameIcon,
                hoveredIcon: _selectedSkillTree.HoveredIcon,
                selectedIcon: _selectedSkillTree.SelectedIcon,
                learnedIcon: _selectedSkillTree.LearnedIcon
            )
        );
    }

    private void UpdateState(EntityUid player)
    {
        _targetPlayer = player;

        if (_window is null)
            return;

        if (!EntityManager.TryGetComponent<CP14SkillStorageComponent>(_targetPlayer, out var storage))
            return;

        //If tree not selected, select the first one
        if (_selectedSkillTree == null)
        {
            var firstTree = _allTrees.First();

            SelectTree(firstTree, storage); // Set the first tree from the player's progress
        }

        if (_selectedSkillTree == null)
            return;

        // Reselect for update state
        SelectNode(_selectedSkill);
        UpdateGraphControl();

        _window.LevelLabel.Text = $"{storage.SkillsSumExperience}/{storage.ExperienceMaxCap}";

        _window.TreeTabsContainer.RemoveAllChildren();
        foreach (var tree in _allTrees)
        {
            float learnedPoints = 0;
            foreach (var skillId in storage.LearnedSkills)
            {
                //TODO: Loop indexing each skill is bad
                if (_proto.TryIndex(skillId, out var skill) && skill.Tree == tree)
                {
                    learnedPoints += skill.LearnCost;
                }
            }

            var treeButton2 = new CP14SkillTreeButtonControl(tree.Color, Loc.GetString(tree.Name), learnedPoints);
            treeButton2.ToolTip = Loc.GetString(tree.Desc ?? string.Empty);
            treeButton2.OnPressed += () =>
            {
                SelectTree(tree, storage);
            };

            _window.TreeTabsContainer.AddChild(treeButton2);
        }
    }

    private void SelectTree(CP14SkillTreePrototype tree, CP14SkillStorageComponent storage)
    {
        if (_window == null)
            return;

        _selectedSkillTree = tree;
        _window.ParallaxBackground.ParallaxPrototype = tree.Parallax;
        _window.TreeName.Text = Loc.GetString(tree.Name);

        UpdateGraphControl();
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
