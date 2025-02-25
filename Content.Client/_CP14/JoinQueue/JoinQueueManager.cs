using Content.Shared._CP14.JoinQueue;
using Robust.Client.State;
using Robust.Shared.Network;

namespace Content.Client._CP14.JoinQueue;

public sealed class JoinQueueManager
{
    [Dependency] private readonly IClientNetManager _netManager = default!;
    [Dependency] private readonly IStateManager _stateManager = default!;

    public void Initialize()
    {
        _netManager.RegisterNetMessage<MsgQueueUpdate>(OnQueueUpdate);
    }

    private void OnQueueUpdate(MsgQueueUpdate msg)
    {
        if (_stateManager.CurrentState is not QueueState)
        {
            _stateManager.RequestStateChange<QueueState>();
        }

        ((QueueState) _stateManager.CurrentState).OnQueueUpdate(msg);
    }
}
