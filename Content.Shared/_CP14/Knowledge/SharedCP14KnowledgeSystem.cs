using Content.Shared._CP14.Knowledge.Components;
using Content.Shared._CP14.Knowledge.Prototypes;
using Content.Shared.DoAfter;
using Content.Shared.MagicMirror;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Knowledge;

public abstract partial class SharedCP14KnowledgeSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
    }

    public bool UseKnowledge(EntityUid uid, ProtoId<CP14KnowledgePrototype> knowledge, float factor = 1f, CP14KnowledgeStorageComponent? knowledgeStorage = null)
    {
        if (!Resolve(uid, ref knowledgeStorage, false))
            return false;

        if (!knowledgeStorage.Knowledges.Contains(knowledge))
            return false;

        var ev = new CP14KnowledgeUsedEvent(uid, knowledge, factor);
        RaiseLocalEvent(uid, ev);
        return true;
    }

    public bool HasKnowledge(EntityUid uid, ProtoId<CP14KnowledgePrototype> knowledge, CP14KnowledgeStorageComponent? knowledgeStorage = null)
    {
        if (!Resolve(uid, ref knowledgeStorage, false))
            return false;

        return knowledgeStorage.Knowledges.Contains(knowledge);
    }
}

public sealed class CP14KnowledgeUsedEvent : EntityEventArgs
{
    public readonly EntityUid User;
    public readonly ProtoId<CP14KnowledgePrototype> Knowledge;
    public readonly float Factor;

    public CP14KnowledgeUsedEvent(EntityUid uid, ProtoId<CP14KnowledgePrototype> knowledge, float factor)
    {
        User = uid;
        Knowledge = knowledge;
        Factor = factor;
    }
}

[Serializable, NetSerializable]
public sealed partial class CP14KnowledgeLearnDoAfterEvent : DoAfterEvent
{
    public ProtoId<CP14KnowledgePrototype> Knowledge;
    public override DoAfterEvent Clone() => this;
}
