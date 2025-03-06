using System.Text;
using Content.Server.Chat.Managers;
using Content.Server.Mind;
using Content.Shared._CP14.Knowledge;
using Content.Shared._CP14.Knowledge.Components;
using Content.Shared._CP14.Knowledge.Events;
using Content.Shared._CP14.Knowledge.Prototypes;
using Content.Shared.Administration.Logs;
using Content.Shared.Chat;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.Mind;
using Content.Shared.Verbs;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Knowledge;

public sealed class CP14KnowledgeSystem : SharedCP14KnowledgeSystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14AutoAddKnowledgeComponent, MapInitEvent>(AutoAddSkill);
        SubscribeLocalEvent<CP14KnowledgeStorageComponent, CP14KnowledgeLearnDoAfterEvent>(KnowledgeLearnedEvent);
        SubscribeLocalEvent<CP14KnowledgeLearningSourceComponent, GetVerbsEvent<Verb>>(AddKnowledgeLearningVerb);

        SubscribeNetworkEvent<CP14RequestKnowledgeInfoEvent>(OnRequestKnowledgeInfoEvent);
    }

    private void AddKnowledgeLearningVerb(Entity<CP14KnowledgeLearningSourceComponent> ent, ref GetVerbsEvent<Verb> args)
    {
        var user = args.User;
        foreach (var knowledge in ent.Comp.Knowledge)
        {
            if (!_prototype.TryIndex(knowledge, out var indexedKnowledge))
                continue;

            args.Verbs.Add(new Verb
            {
                Text = $"{Loc.GetString(indexedKnowledge.Name)}",
                Message = Loc.GetString(indexedKnowledge.Desc),
                Category = VerbCategory.CP14KnowledgeLearn,
                Act = () =>
                {
                    _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager,
                        user,
                        ent.Comp.DoAfter,
                        new CP14KnowledgeLearnDoAfterEvent { Knowledge = knowledge },
                        user,
                        ent,
                        ent)
                    {
                        BreakOnDamage = true,
                        BreakOnMove = true,
                    });
                },
                Disabled = HasKnowledge(user, knowledge),
                Impact = LogImpact.Medium,
            });
        }
    }

    private void KnowledgeLearnedEvent(Entity<CP14KnowledgeStorageComponent> ent, ref CP14KnowledgeLearnDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        args.Handled = true;
        TryAdd(ent.Owner, args.Knowledge);
    }

    private void AutoAddSkill(Entity<CP14AutoAddKnowledgeComponent> ent, ref MapInitEvent args)
    {
        foreach (var knowledge in ent.Comp.Knowledge)
        {
            TryAdd(ent.Owner, knowledge);
        }

        RemComp(ent, ent.Comp);
    }

    public bool TryAdd(Entity<CP14KnowledgeStorageComponent?> entity, ProtoId<CP14KnowledgePrototype> knowledgeId, bool force = false, bool silent = false)
    {
        if (!Resolve(entity, ref entity.Comp, false))
            return false;

        if (HasKnowledge(entity, knowledgeId))
            return false;

        if (!_prototype.TryIndex(knowledgeId, out var knowledge))
            return false;

        MindComponent? mindComponent;

        foreach (var dependencyKnowledgeId in knowledge.Dependencies)
        {
            if (!_prototype.TryIndex(dependencyKnowledgeId, out var dependencyKnowledge))
                return false;

            // If we teach by force - we automatically teach all the basics that are necessary for that skill.
            if (force && !TryAdd(entity, dependencyKnowledge, true))
                return false;

            var sb = new StringBuilder();
            sb.Append(Loc.GetString("cp14-cant-learn-knowledge-dependencies", ("target", Loc.GetString(knowledge.Desc))));

            // We cant learnt
            if (HasKnowledge(entity, dependencyKnowledge))
                continue;

            sb.Append($"\n- {Loc.GetString(dependencyKnowledge.Desc)}");

            if (silent)
                return false;

            if (!_mind.TryGetMind(entity, out _, out mindComponent) || mindComponent.Session is null)
                return false;

            var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", sb.ToString()));
            _chat.ChatMessageToOne(
                ChatChannel.Server,
                sb.ToString(),
                wrappedMessage,
                default,
                false,
                mindComponent.Session.Channel);

            return false;
        }

        if (!entity.Comp.Knowledge.Add(knowledgeId))
            return false;

        _adminLogger.Add(LogType.Mind, LogImpact.Medium, $"{EntityManager.ToPrettyString(entity):player} learned new knowledge: {Loc.GetString(knowledge.Name)}");

        // TODO: coding on a sleepy head is a bad idea. Remove duplicate variables. >:(
        if (silent)
            return true;

        if (!_mind.TryGetMind(entity, out _, out mindComponent) || mindComponent.Session is null)
            return false;

        var message = Loc.GetString("cp14-learned-new-knowledge", ("name", Loc.GetString(knowledge.Name)));
        var wrappedMessage2 = Loc.GetString("chat-manager-server-wrap-message", ("message", message));

        _chat.ChatMessageToOne(
            ChatChannel.Server,
            message,
            wrappedMessage2,
            default,
            false,
            mindComponent.Session.Channel);

        return true;
    }

    public bool TryRemove(Entity<CP14KnowledgeStorageComponent?> entity, ProtoId<CP14KnowledgePrototype> proto, bool silent = false)
    {
        if (!Resolve(entity, ref entity.Comp, false))
            return false;

        if (!entity.Comp.Knowledge.Contains(proto))
            return false;

        if (!_prototype.TryIndex(proto, out var indexedKnowledge))
            return false;

        if (!entity.Comp.Knowledge.Remove(proto))
            return false;

        _adminLogger.Add(LogType.Mind, LogImpact.Medium, $"{EntityManager.ToPrettyString(entity):player} forgot knowledge: {Loc.GetString(indexedKnowledge.Name)}");

        if (silent)
            return true;

        if (!_mind.TryGetMind(entity, out _, out var mindComp) || mindComp.Session is null)
            return true;

        var message = Loc.GetString("cp14-forgot-knowledge", ("name", Loc.GetString(indexedKnowledge.Name)));
        var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", message));

        _chat.ChatMessageToOne(
            ChatChannel.Server,
            message,
            wrappedMessage,
            default,
            false,
            mindComp.Session.Channel);

        return true;
    }

    private void OnRequestKnowledgeInfoEvent(CP14RequestKnowledgeInfoEvent msg, EntitySessionEventArgs args)
    {
        if (!args.SenderSession.AttachedEntity.HasValue || args.SenderSession.AttachedEntity != GetEntity(msg.NetEntity))
            return;

        var entity = args.SenderSession.AttachedEntity.Value;

        if (!TryComp<CP14KnowledgeStorageComponent>(entity, out var knowledgeComp))
            return;

        RaiseNetworkEvent(new CP14KnowledgeInfoEvent(GetNetEntity(entity),knowledgeComp.Knowledge), args.SenderSession);
    }
}
