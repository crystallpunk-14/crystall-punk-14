using System.Text;
using Content.Shared._CP14.Knowledge.Components;
using Content.Shared._CP14.Knowledge.Prototypes;
using Content.Shared.DoAfter;
using Content.Shared.Ghost;
using Content.Shared.MagicMirror;
using Content.Shared.Paper;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Knowledge;

public abstract partial class SharedCP14KnowledgeSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly PaperSystem _paper = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CP14KnowledgePaperTextComponent, MapInitEvent>(OnPaperMapInit);
    }

    private void OnPaperMapInit(Entity<CP14KnowledgePaperTextComponent> ent, ref MapInitEvent args)
    {
        if (!TryComp<PaperComponent>(ent, out var paper))
            return;
        if (!TryComp<CP14KnowledgeLearningSourceComponent>(ent, out var knowledge))
            return;
        if (knowledge.Knowledges.Count <= 0)
            return;

        var sb = new StringBuilder();

        sb.Append(Loc.GetString("cp14-knowledge-book-pre-text"));
        foreach (var k in knowledge.Knowledges)
        {
            if (!_proto.TryIndex(k, out var indexedKnowledge))
                continue;

            sb.Append($"\n{Loc.GetString(indexedKnowledge.Desc)}");
        }

        sb.Append($"\n\n{Loc.GetString("cp14-knowledge-book-post-text")}");

        _paper.SetContent((ent, paper), sb.ToString());
        paper.EditingDisabled = true;
    }

    public bool UseKnowledge(EntityUid uid,
        ProtoId<CP14KnowledgePrototype> knowledge,
        float factor = 1f,
        CP14KnowledgeStorageComponent? knowledgeStorage = null)
    {
        if (!Resolve(uid, ref knowledgeStorage, false))
            return false;

        if (!knowledgeStorage.Knowledges.Contains(knowledge))
            return false;

        var ev = new CP14KnowledgeUsedEvent(uid, knowledge, factor);
        RaiseLocalEvent(uid, ev);
        return true;
    }

    public bool HasKnowledge(EntityUid uid,
        ProtoId<CP14KnowledgePrototype> knowledge,
        CP14KnowledgeStorageComponent? knowledgeStorage = null)
    {
        if (HasComp<GhostComponent>(uid)) //All-knowing ghosts
            return true;
        
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

[Serializable, NetSerializable]
public sealed class CP14KnowledgeInfoEvent : EntityEventArgs
{
    public readonly NetEntity NetEntity;
    public readonly HashSet<ProtoId<CP14KnowledgePrototype>> AllKnowledge;

    public CP14KnowledgeInfoEvent(NetEntity netEntity, HashSet<ProtoId<CP14KnowledgePrototype>> allKnowledge)
    {
        NetEntity = netEntity;
        AllKnowledge = allKnowledge;
    }
}

[Serializable, NetSerializable]
public sealed class RequestKnowledgeInfoEvent : EntityEventArgs
{
    public readonly NetEntity NetEntity;

    public RequestKnowledgeInfoEvent(NetEntity netEntity)
    {
        NetEntity = netEntity;
    }
}
