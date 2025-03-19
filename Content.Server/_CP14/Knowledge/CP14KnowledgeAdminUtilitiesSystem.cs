using Content.Shared._CP14.Knowledge.Components;
using Content.Shared._CP14.Knowledge.Prototypes;
using Content.Shared.Administration;
using Content.Shared.Administration.Managers;
using Content.Shared.Database;
using Content.Shared.Verbs;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Knowledge;

// TODO: Add UI
public sealed class CP14KnowledgeAdminUtilitiesSystem : EntitySystem
{
    [Dependency] private readonly ISharedAdminManager _admin = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly CP14KnowledgeSystem _knowledge = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14KnowledgeStorageComponent, GetVerbsEvent<Verb>>(AddKnowledgeAdminVerb);
    }

    private void AddKnowledgeAdminVerb(Entity<CP14KnowledgeStorageComponent> entity, ref GetVerbsEvent<Verb> args)
    {
        if (!_admin.HasAdminFlag(args.User, AdminFlags.Admin))
            return;

        // Remove knowledge
        foreach (var knowledge in entity.Comp.Knowledge)
        {
            if (!_prototype.TryIndex(knowledge, out var indexedKnowledge))
                continue;

            args.Verbs.Add(new Verb
            {
                Text = $"{Loc.GetString(indexedKnowledge.Name)}",
                Message = Loc.GetString(indexedKnowledge.Desc),
                Category = VerbCategory.CP14KnowledgeRemove,
                Act = () =>
                {
                    _knowledge.TryRemove(entity.Owner, knowledge);
                },
                Impact = LogImpact.High,
            });
        }

        // Add knowledge
        foreach (var knowledge in _prototype.EnumeratePrototypes<CP14KnowledgePrototype>())
        {
            // An entity with CP14AllKnowingComponent can't be taught anything,
            // it already knows everything
            if (_knowledge.HasKnowledge(entity.Owner, knowledge.ID))
                continue;

            args.Verbs.Add(new Verb
            {
                Text = $"{Loc.GetString(knowledge.Name)}",
                Message = Loc.GetString(knowledge.Desc),
                Category = VerbCategory.CP14KnowledgeAdd,
                Act = () =>
                {
                    _knowledge.TryAdd(entity.Owner, knowledge, true);
                },
                Impact = LogImpact.High,
            });
        }
    }

}
