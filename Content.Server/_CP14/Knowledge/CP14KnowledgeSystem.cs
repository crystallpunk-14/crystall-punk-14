using Content.Server.Chat.Managers;
using Content.Server.Mind;
using Content.Server.Popups;
using Content.Shared._CP14.Knowledge;
using Content.Shared._CP14.Knowledge.Components;
using Content.Shared._CP14.Knowledge.Prototypes;
using Content.Shared.Administration;
using Content.Shared.Administration.Managers;
using Content.Shared.Chat;
using Content.Shared.Damage;
using Content.Shared.Database;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Throwing;
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
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly MindSystem _mind = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CP14AutoAddKnowledgeComponent, MapInitEvent>(AutoAddSkill);
        SubscribeLocalEvent<CP14KnowledgeStorageComponent, GetVerbsEvent<Verb>>(AddKnowledgeAdminVerb);
    }

    private void AddKnowledgeAdminVerb(Entity<CP14KnowledgeStorageComponent> ent, ref GetVerbsEvent<Verb> args)
    {
        if (!_admin.HasAdminFlag(args.User, AdminFlags.Admin))
            return;

        //Remove knowledge
        foreach (var knowledge in ent.Comp.Knowledges)
        {
            args.Verbs.Add(new Verb()
            {
                Text = $"Remove: {knowledge.Id}",
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
                Text = $"Add: {knowledge.ID}",
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

    public bool TryLearnKnowledge(EntityUid uid, ProtoId<CP14KnowledgePrototype> proto, bool force = false)
    {
        if (!TryComp<CP14KnowledgeStorageComponent>(uid, out var knowledgeStorage))
            return false;

        if (!_proto.TryIndex(proto, out var indexedKnowledge))
            return false;

        if (force)
        {
            //If we teach by force - we automatically teach all the basics that are necessary for that skill.
            foreach (var dependency in indexedKnowledge.Dependencies)
            {
                if (!TryLearnKnowledge(uid, dependency, true))
                    return false;
            }
        }

        if (_mind.TryGetMind(uid, out var mind, out var mindComp) && mindComp.Session is not null)
        {
            var message = Loc.GetString("cp14-learned-new-knowledge", ("name", Loc.GetString(indexedKnowledge.Name)));
            var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", message));

            _chat.ChatMessageToOne(
                ChatChannel.Server,
                message,
                wrappedMessage,
                default,
                false,
                mindComp.Session.Channel);
        }

        //TODO: Logging
        return knowledgeStorage.Knowledges.Add(proto);
    }

    public bool TryForgotKnowledge(EntityUid uid, ProtoId<CP14KnowledgePrototype> proto)
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

        //TODO: Logging
        return true;
    }
}
