using Content.Shared._CP14.Knowledge;
using Content.Shared._CP14.Knowledge.Events;
using Content.Shared._CP14.Knowledge.Prototypes;
using Robust.Client.Player;
using Robust.Shared.Prototypes;

namespace Content.Client._CP14.Knowledge;

public sealed class ClientCP14KnowledgeSystem : SharedCP14KnowledgeSystem
{
    [Dependency] private readonly IPlayerManager _players = default!;

    public event Action<KnowledgeData>? OnKnowledgeUpdate;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<CP14KnowledgeInfoEvent>(OnCharacterKnowledgeEvent);
    }

    public void RequestKnowledgeInfo()
    {
        var entity = _players.LocalEntity;
        if (entity is null)
            return;

        RaiseNetworkEvent(new CP14RequestKnowledgeInfoEvent(GetNetEntity(entity.Value)));
    }

    private void OnCharacterKnowledgeEvent(CP14KnowledgeInfoEvent msg, EntitySessionEventArgs args)
    {
        var entity = GetEntity(msg.NetEntity);
        var data = new KnowledgeData(entity, msg.AllKnowledge);

        OnKnowledgeUpdate?.Invoke(data);
    }

    public readonly record struct KnowledgeData(
        EntityUid Entity,
        HashSet<ProtoId<CP14KnowledgePrototype>> AllKnowledge
    );
}
