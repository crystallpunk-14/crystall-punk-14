using Content.Shared.CCVar;
using Content.Shared.Chat;
using Content.Shared.Communications;
using Robust.Client.UserInterface;
using Robust.Shared.Configuration;
using Robust.Shared.Timing;

namespace Content.Client._CP14.UserInterface
{
    public sealed class GuardBellBoundUserInterface : BoundUserInterface
    {
        [Dependency] private readonly IConfigurationManager _cfg = default!;

        [ViewVariables]
        private GuardBellMenu? _menu;

        public GuardBellBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
        {
        }

        protected override void Open()
        {
            base.Open();

            _menu = this.CreateWindow<GuardBellMenu>();
            _menu.OnAlertLevel += AlertLevelSelected;
        }

        public void AlertLevelSelected(string level)
        {
            if (_menu!.AlertLevelSelectable)
            {
                _menu.CurrentLevel = level;
                SendMessage(new CommunicationsConsoleSelectAlertLevelMessage(level));
            }
        }


        protected override void UpdateState(BoundUserInterfaceState state)
        {
            base.UpdateState(state);

            if (state is not CommunicationsConsoleInterfaceState commsState)
                return;

            if (_menu != null)
            {
                _menu.AlertLevelSelectable = commsState.AlertLevels != null && !float.IsNaN(commsState.CurrentAlertDelay) && commsState.CurrentAlertDelay <= 0;
                _menu.CurrentLevel = commsState.CurrentAlert;
                _menu.UpdateAlertLevels(commsState.AlertLevels, _menu.CurrentLevel);
                _menu.AlertLevelButton.Disabled = !_menu.AlertLevelSelectable;
            }
        }
    }
}
