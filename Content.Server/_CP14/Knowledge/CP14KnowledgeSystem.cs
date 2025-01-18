using System.Text;
using Content.Server.Chat.Managers;
using Content.Server.Mind;
using Content.Server.Popups;
using Content.Shared._CP14.Knowledge;
using Content.Shared._CP14.Knowledge.Components;
using Content.Shared._CP14.Knowledge.Prototypes;
using Content.Shared.Administration;
using Content.Shared.Administration.Logs;
using Content.Shared.Administration.Managers;
using Content.Shared.Chat;
using Content.Shared.Damage;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.Verbs;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._CP14.Knowledge;

public sealed partial class CP14KnowledgeSystem : SharedCP14KnowledgeSystem
{
    [Dependency] private readonly ISharedAdminManager _admin = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14AutoAddKnowledgeComponent, MapInitEvent>(AutoAddSkill);
        SubscribeLocalEvent<CP14KnowledgeStorageComponent, GetVerbsEvent<Verb>>(AddKnowledgeAdminVerb);
        SubscribeLocalEvent<CP14KnowledgeLearningSourceComponent, GetVerbsEvent<Verb>>(AddKnowledgeLearningVerb);
        SubscribeLocalEvent<CP14KnowledgeStorageComponent, CP14KnowledgeLearnDoAfterEvent>(KnowledgeLearnedEvent);

        SubscribeNetworkEvent<RequestKnowledgeInfoEvent>(OnRequestKnowledgeInfoEvent);
    }

    private void KnowledgeLearnedEvent(Entity<CP14KnowledgeStorageComponent> ent, ref CP14KnowledgeLearnDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        args.Handled = true;

        TryLearnKnowledge(ent, args.Knowledge);
    }

    private void AddKnowledgeLearningVerb(Entity<CP14KnowledgeLearningSourceComponent> ent, ref GetVerbsEvent<Verb> args)
    {
        var user = args.User;
        foreach (var knowledge in ent.Comp.Knowledges)
        {
            if (!_proto.TryIndex(knowledge, out var indexedKnowledge))
                continue;

            args.Verbs.Add(new Verb()
            {
                Text = $"{Loc.GetString(indexedKnowledge.Name)}",
                Message = Loc.GetString(indexedKnowledge.Desc),
                Category = VerbCategory.CP14KnowledgeLearn,
                Act = () =>
                {
                    _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager,
                        user,
                        ent.Comp.DoAfter,
                        new CP14KnowledgeLearnDoAfterEvent() {Knowledge = knowledge},
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

    private void AddKnowledgeAdminVerb(Entity<CP14KnowledgeStorageComponent> ent, ref GetVerbsEvent<Verb> args)
    {
        if (!_admin.HasAdminFlag(args.User, AdminFlags.Admin))
            return;

        //Remove knowledge
        foreach (var knowledge in ent.Comp.Knowledges)
        {
            if (!_proto.TryIndex(knowledge, out var indexedKnowledge))
                continue;

            args.Verbs.Add(new Verb()
            {
                Text = $"{Loc.GetString(indexedKnowledge.Name)}",
                Message = Loc.GetString(indexedKnowledge.Desc),
                Category = VerbCategory.CP14KnowledgeRemove,
                Act = () =>
                {
                    TryForgotKnowledge(ent, knowledge);
                },
                Impact = LogImpact.High,
            });
        }

        //Add knowledge
        foreach (var knowledge in _proto.EnumeratePrototypes<CP14KnowledgePrototype>())
        {
            if (ent.Comp.Knowledges.Contains(knowledge))
                continue;

            args.Verbs.Add(new Verb()
            {
                Text = $"{Loc.GetString(knowledge.Name)}",
                Message = Loc.GetString(knowledge.Desc),
                Category = VerbCategory.CP14KnowledgeAdd,
                Act = () =>
                {
                    TryLearnKnowledge(ent, knowledge, true);
                },
                Impact = LogImpact.High,
            });
        }
    }

    private void AutoAddSkill(Entity<CP14AutoAddKnowledgeComponent> ent, ref MapInitEvent args)
    {
        foreach (var knowledge in ent.Comp.Knowledge)
        {
            TryLearnKnowledge(ent, knowledge);
        }

        RemComp(ent, ent.Comp);
    }

    public bool TryLearnKnowledge(EntityUid uid, ProtoId<CP14KnowledgePrototype> proto, bool force = false, bool silent = false)
    {
        if (!TryComp<CP14KnowledgeStorageComponent>(uid, out var knowledgeStorage))
            return false;

        if (!_proto.TryIndex(proto, out var indexedKnowledge))
            return false;

        if (knowledgeStorage.Knowledges.Contains(proto))
            return false;

        foreach (var dependency in indexedKnowledge.Dependencies)
        {
            if (!_proto.TryIndex(dependency, out var indexedDependency))
                return false;

            if (force)
            {
                //If we teach by force - we automatically teach all the basics that are necessary for that skill.
                if (!TryLearnKnowledge(uid, dependency, true))
                    return false;
            }
            else
            {
                var passed = true;
                var sb = new StringBuilder();

                sb.Append(Loc.GetString("cp14-cant-learn-knowledge-dependencies",
                    ("target", Loc.GetString(indexedKnowledge.Desc))));

                //We cant learnt
                if (!HasKnowledge(uid, dependency))
                {
                    passed = false;
                    sb.Append("\n- " + Loc.GetString(indexedDependency.Desc));
                }

                if (!passed)
                {
                    if (!silent && _mind.TryGetMind(uid, out var mind, out var mindComp) && mindComp.Session is not null)
                    {
                        var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", sb.ToString()));
                        _chat.ChatMessageToOne(
                            ChatChannel.Server,
                            sb.ToString(),
                            wrappedMessage,
                            default,
                            false,
                            mindComp.Session.Channel);
                    }

                    return false;
                }
            }
        }

        //TODO: coding on a sleepy head is a bad idea. Remove duplicate variables. >:(
        if (!silent && _mind.TryGetMind(uid, out var mind2, out var mindComp2) && mindComp2.Session is not null)
        {
            var message = Loc.GetString("cp14-learned-new-knowledge", ("name", Loc.GetString(indexedKnowledge.Name)));
            var wrappedMessage2 = Loc.GetString("chat-manager-server-wrap-message", ("message", message));

            _chat.ChatMessageToOne(
                ChatChannel.Server,
                message,
                wrappedMessage2,
                default,
                false,
                mindComp2.Session.Channel);
        }

        _adminLogger.Add(
            LogType.Mind,
            LogImpact.Medium,
            $"{EntityManager.ToPrettyString(uid):player} learned new knowledge: {Loc.GetString(indexedKnowledge.Name)}");
        return knowledgeStorage.Knowledges.Add(proto);
    }

    public bool TryForgotKnowledge(EntityUid uid, ProtoId<CP14KnowledgePrototype> proto, bool silent = false)
    {
        if (!TryComp<CP14KnowledgeStorageComponent>(uid, out var knowledgeStorage))
            return false;

        if (!knowledgeStorage.Knowledges.Contains(proto))
            return false;

        if (!_proto.TryIndex(proto, out var indexedKnowledge))
            return false;

        knowledgeStorage.Knowledges.Remove(proto);

        if (_mind.TryGetMind(uid, out var mind, out var mindComp) && mindComp.Session is not null)
        {
            if (!silent)
            {
                var message = Loc.GetString("cp14-forgot-knowledge", ("name", Loc.GetString(indexedKnowledge.Name)));
                var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", message));

                _chat.ChatMessageToOne(
                    ChatChannel.Server,
                    message,
                    wrappedMessage,
                    default,
                    false,
                    mindComp.Session.Channel);
            }
        }

        _adminLogger.Add(
            LogType.Mind,
            LogImpact.Medium,
            $"{EntityManager.ToPrettyString(uid):player} forgot knowledge: {Loc.GetString(indexedKnowledge.Name)}");
        return true;
    }

    private void OnRequestKnowledgeInfoEvent(RequestKnowledgeInfoEvent msg, EntitySessionEventArgs args)
    {
        if (!args.SenderSession.AttachedEntity.HasValue || args.SenderSession.AttachedEntity != GetEntity(msg.NetEntity))
            return;

        var entity = args.SenderSession.AttachedEntity.Value;

        if (!TryComp<CP14KnowledgeStorageComponent>(entity, out var knowledgeComp))
            return;

        RaiseNetworkEvent(new CP14KnowledgeInfoEvent(GetNetEntity(entity),knowledgeComp.Knowledges), args.SenderSession);
    }
}
