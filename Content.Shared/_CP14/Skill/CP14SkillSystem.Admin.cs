using Content.Shared._CP14.Skill.Components;
using Content.Shared._CP14.Skill.Prototypes;
using Content.Shared.Administration;
using Content.Shared.Administration.Managers;
using Content.Shared.Verbs;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Skill;

public abstract partial class CP14SharedSkillSystem
{
    [Dependency] private readonly ISharedAdminManager _admin = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    private void InitializeAdmin()
    {
        SubscribeLocalEvent<CP14SkillStorageComponent, GetVerbsEvent<Verb>>(OnGetAdminVerbs);
    }

    private void OnGetAdminVerbs(Entity<CP14SkillStorageComponent> ent, ref GetVerbsEvent<Verb> args)
    {
        if (!_admin.HasAdminFlag(args.User, AdminFlags.Admin))
            return;


        var target = args.Target;

        //Add Skill
        foreach (var skill in _proto.EnumeratePrototypes<CP14SkillPrototype>())
        {
            if (ent.Comp.LearnedSkills.Contains(skill))
                continue;

            var name = Loc.GetString(GetSkillName(skill));
            args.Verbs.Add(new Verb
            {
                Text = name,
                Message = name + ": " + Loc.GetString(GetSkillDescription(skill)),
                Category = VerbCategory.CP14AdminSkillAdd,
                Icon = skill.Icon,
                Act = () =>
                {
                    TryAddSkill(target, skill);
                },
            });
        }

        //Remove Skill
        foreach (var skill in ent.Comp.LearnedSkills)
        {
            if (!_proto.TryIndex(skill, out var indexedSkill))
                continue;

            var name = Loc.GetString(GetSkillName(skill));
            args.Verbs.Add(new Verb
            {
                Text = name,
                Message = name + ": " + Loc.GetString(GetSkillDescription(skill)),
                Category = VerbCategory.CP14AdminSkillRemove,
                Icon = indexedSkill.Icon,
                Act = () =>
                {
                    TryRemoveSkill(target, skill);
                },
            });
        }
    }
}
