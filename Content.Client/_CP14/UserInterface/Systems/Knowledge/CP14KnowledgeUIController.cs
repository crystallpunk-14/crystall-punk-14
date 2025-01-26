using Content.Client._CP14.Knowledge;
using Content.Client._CP14.UserInterface.Systems.Knowledge.Windows;
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
public sealed class CP14KnowledgeUIController : UIController, IOnStateEntered<GameplayState>, IOnStateExited<GameplayState>,
    IOnSystemChanged<ClientCP14KnowledgeSystem>
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [UISystemDependency] private readonly ClientCP14KnowledgeSystem _knowledge = default!;

    private CP14KnowledgeWindow? _window;

    private MenuButton? KnowledgeButton => UIManager.GetActiveUIWidgetOrNull<MenuBar.Widgets.GameTopMenuBar>()?.CP14KnowledgeButton;

    public void OnStateEntered(GameplayState state)
    {
        DebugTools.Assert(_window == null);

        _window = UIManager.CreateWindow<CP14KnowledgeWindow>();
        LayoutContainer.SetAnchorPreset(_window, LayoutContainer.LayoutPreset.CenterTop);

        CommandBinds.Builder
            .Bind(ContentKeyFunctions.CP14OpenKnowledgeMenu,
                InputCmdHandler.FromDelegate(_ => ToggleWindow()))
            .Register<CP14KnowledgeUIController>();
    }

    public void OnStateExited(GameplayState state)
    {
        if (_window != null)
        {
            _window.Dispose();
            _window = null;
        }

        CommandBinds.Unregister<CP14KnowledgeUIController>();
    }

    public void OnSystemLoaded(ClientCP14KnowledgeSystem system)
    {
        system.OnKnowledgeUpdate += KnowledgeUpdated;
        _player.LocalPlayerDetached += CharacterDetached;
    }

    public void OnSystemUnloaded(ClientCP14KnowledgeSystem system)
    {
        system.OnKnowledgeUpdate -= KnowledgeUpdated;
        _player.LocalPlayerDetached -= CharacterDetached;
    }

    public void UnloadButton()
    {
        if (KnowledgeButton is null)
            return;

        KnowledgeButton.OnPressed -= KnowledgeButtonPressed;
    }

    public void LoadButton()
    {
        if (KnowledgeButton is null)
            return;

        KnowledgeButton.OnPressed += KnowledgeButtonPressed;

        if (_window is null)
            return;

        _window.OnClose += DeactivateButton;
        _window.OnOpen += ActivateButton;
    }

    private void DeactivateButton()
    {
        KnowledgeButton!.Pressed = false;
    }

    private void ActivateButton()
    {
        KnowledgeButton!.Pressed = true;
    }

    private void KnowledgeUpdated(ClientCP14KnowledgeSystem.KnowledgeData data)
    {
        if (_window is null)
            return;

        _window.KnowledgeContent.RemoveAllChildren();

        var (entity, allKnowledge) = data;

        foreach (var knowledge in allKnowledge)
        {
            if (!_proto.TryIndex(knowledge, out var indexedKnowledge))
                continue;

            var knowledgeButton = new Button()
            {
                Access = AccessLevel.Public,
                Text = Loc.GetString(indexedKnowledge.Name),
                ToolTip = Loc.GetString(indexedKnowledge.Desc),
                TextAlign = Label.AlignMode.Center,
            };

            _window.KnowledgeContent.AddChild(knowledgeButton);
        }
    }

    private void CharacterDetached(EntityUid uid)
    {
        CloseWindow();
    }

    private void KnowledgeButtonPressed(BaseButton.ButtonEventArgs args)
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

        if (KnowledgeButton != null)
        {
            KnowledgeButton.SetClickPressed(!_window.IsOpen);
        }

        if (_window.IsOpen)
        {
            CloseWindow();
        }
        else
        {
            _knowledge.RequestKnowledgeInfo();
            _window.Open();
        }
    }
}
