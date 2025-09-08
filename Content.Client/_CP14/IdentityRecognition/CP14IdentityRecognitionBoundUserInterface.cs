using Content.Shared._CP14.IdentityRecognition;
using Content.Shared.Labels.Components;
using Content.Shared.Mind.Components;
using Robust.Client.Player;
using Robust.Client.UserInterface;

namespace Content.Client._CP14.IdentityRecognition;

public sealed class CP14IdentityRecognitionBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    [ViewVariables]
    private CP14RememberNameWindow? _window;

    private NetEntity? _rememberedTarget;

    public CP14IdentityRecognitionBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<CP14RememberNameWindow>();

        if (_entManager.TryGetComponent(Owner, out HandLabelerComponent? labeler))
        {
            _window.SetMaxLabelLength(labeler!.MaxLabelChars);
        }

        _window.OnRememberedNameChanged += OnLabelChanged;
        Reload();
    }

    private void OnLabelChanged(string newLabel)
    {
        if (_rememberedTarget is null)
            return;

        // Focus moment
        var currentName = CurrentName();

        if (currentName is not null && currentName.Equals(newLabel))
            return;

        SendPredictedMessage(new CP14RememberedNameChangedMessage(newLabel, _rememberedTarget.Value));
    }

    public void Reload()
    {
        if (_window is null)
            return;

        var currentName = CurrentName();

        if (currentName is null)
            return;

        _window.SetCurrentLabel(currentName);
    }

    private string? CurrentName()
    {
        if (_rememberedTarget is null)
            return null;
        if (!_entManager.TryGetComponent<MindContainerComponent>(_player.LocalEntity, out var mindContainer))
            return null;
        if (!_entManager.TryGetComponent<CP14RememberedNamesComponent>(mindContainer.Mind, out var knownNames))
            return null;

        var netId = _rememberedTarget.Value.Id;
        return knownNames.Names.GetValueOrDefault(netId);
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (_window is null)
            return;

        switch (state)
        {
            case CP14RememberNameUiState rememberNameUiState:
                _rememberedTarget = rememberNameUiState.Target;

                var currentName = CurrentName();
                if (currentName is not null)
                    _window.SetCurrentLabel(currentName);
                break;
        }
    }
}
