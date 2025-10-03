using Content.Client.Hands.Systems;
using Content.Client.UserInterface.Screens;
using Content.Shared._CP14.Runes;
using Content.Shared._CP14.Runes.Components;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Timing;

namespace Content.Client._CP14.Runes;

public sealed class CP14DrawingRuneSystem : CP14SharedDrawingRuneSystem
{
    [Dependency] private readonly IUserInterfaceManager _userInterfaceManager = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly HandsSystem _handsSystem = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly EntityManager _entityManager = default!;

    private Popup? _drawingPopup;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if(!_gameTiming.IsFirstTimePredicted)
            return;

        var heldUid = _handsSystem.GetActiveHandEntity();

        if(!TryComp<CP14RuneDrawingToolComponent>(heldUid, out var runeDrawingToolComponent))
            return;

        if(runeDrawingToolComponent.ChosenSpell == null)
            return;

        if(runeDrawingToolComponent.HasEnoughMana == false)
            return;

        //TODO: Make this shit work

    }

    private void OpenPopup(CP14RuneDrawingToolComponent runeDrawingToolComponent, EntityManager heldEntity)
    {
        if (_drawingPopup != null)
            return;


        throw new NotImplementedException();
    }

    private void ClosePopup()
    {
        if (_drawingPopup is null)
            return;

        switch (_userInterfaceManager.ActiveScreen)
        {
            case DefaultGameScreen gameScreen:
                gameScreen.RemoveChild(_drawingPopup);
                break;

            case SeparatedChatGameScreen gameScreen:
                gameScreen.RemoveChild(_drawingPopup);
                break;
        }

        _drawingPopup = null;
    }
}
