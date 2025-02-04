using Content.Shared._CP14.Discord;
using Robust.Client.State;
using Robust.Shared.Network;

namespace Content.Client._CP14.Discord;

public sealed class DiscordAuthManager
{
    [Dependency] private readonly IClientNetManager _netManager = default!;
    [Dependency] private readonly IStateManager _stateManager = default!;

    public string AuthUrl { get; private set; } = "";

    public void Initialize()
    {
        _netManager.RegisterNetMessage<MsgDiscordAuthCheck>();
        _netManager.RegisterNetMessage<MsgDiscordAuthRequired>(OnDiscordAuthRequired);
    }

    private void OnDiscordAuthRequired(MsgDiscordAuthRequired msg)
    {
        if (_stateManager.CurrentState is DiscordAuthState)
            return;
        AuthUrl = msg.AuthUrl;
        _stateManager.RequestStateChange<DiscordAuthState>();
    }
}
