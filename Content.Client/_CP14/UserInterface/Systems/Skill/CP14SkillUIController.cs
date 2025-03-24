using Content.Client._CP14.Skill;
using Content.Client._CP14.UserInterface.Systems.Skill.Window;
using Content.Client.Gameplay;
using Content.Client.UserInterface.Controls;
using Content.Shared.Input;
using JetBrains.Annotations;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
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
    }

    public void OnStateExited(GameplayState state)
    {
        if (_window != null)
        {
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

    private void SkillUpdate(SkillMenuData data)
    {
        if (_window is null)
            return;

        _window.SkillContent.RemoveAllChildren();

        var (entity, skills) = data;

        foreach (var skill in skills)
        {
            if (!_proto.TryIndex(skill, out var indexedSkill))
                continue;

            var knowledgeButton = new Button()
            {
                Access = AccessLevel.Public,
                Text = Loc.GetString(indexedSkill.Name),
                ToolTip = Loc.GetString(indexedSkill.Desc ?? string.Empty),
                TextAlign = Label.AlignMode.Center,
            };

            _window.SkillContent.AddChild(knowledgeButton);
        }
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
