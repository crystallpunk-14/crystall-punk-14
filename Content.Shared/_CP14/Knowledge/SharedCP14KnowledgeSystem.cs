using Content.Shared._CP14.Knowledge.Components;
using Content.Shared._CP14.Knowledge.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared._CP14.Knowledge;

public abstract partial class SharedCP14KnowledgeSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
    }

    public bool TryUseKnowledge(EntityUid uid, ProtoId<CP14KnowledgePrototype> knowledge, CP14KnowledgeStorageComponent? knowledgeStorage = null)
    {
        if (!Resolve(uid, ref knowledgeStorage, false))
            return false;

        if (!knowledgeStorage.Knowledges.Contains(knowledge))
            return false;

        var ev = new CP14KnowledgeUsedEvent(uid, knowledge);
        RaiseLocalEvent(uid, ev);
        return true;
    }
}

public sealed class CP14TrySkillIssueEvent : EntityEventArgs
{
    public readonly EntityUid User;

    public CP14TrySkillIssueEvent(EntityUid uid)
    {
        User = uid;
    }
}

public sealed class CP14KnowledgeUsedEvent : EntityEventArgs
{
    public readonly EntityUid User;
    public readonly ProtoId<CP14KnowledgePrototype> Knowledge;

    public CP14KnowledgeUsedEvent(EntityUid uid, ProtoId<CP14KnowledgePrototype> knowledge)
    {
        User = uid;
        Knowledge = knowledge;
    }
}
