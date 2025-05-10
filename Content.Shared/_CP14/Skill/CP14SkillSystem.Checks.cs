using System.Text;
using Content.Shared._CP14.Skill.Components;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Network;
using Robust.Shared.Random;

namespace Content.Shared._CP14.Skill;

public abstract partial class CP14SharedSkillSystem
{
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    private void InitializeChecks()
    {
        SubscribeLocalEvent<CP14MeleeWeaponSkillRequiredComponent, MeleeHitEvent>(OnMeleeAttack);
        SubscribeLocalEvent<CP14MeleeWeaponSkillRequiredComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(Entity<CP14MeleeWeaponSkillRequiredComponent> ent, ref ExaminedEvent args)
    {
        var sb = new StringBuilder();
        sb.Append(Loc.GetString("cp14-skill-issue-title") + "\n");

        foreach (var skill in ent.Comp.Skills)
        {
            if (!_proto.TryIndex(skill, out var indexedSkill))
                continue;

            var color = HaveSkill(args.Examiner, skill) ? Color.LimeGreen.ToHex() : Color.Red.ToHex();
            sb.Append($"[color={color}] - {Loc.GetString(indexedSkill.Name)} [/color]\n");
        }
        args.PushMarkup(sb.ToString());
    }

    private void OnMeleeAttack(Entity<CP14MeleeWeaponSkillRequiredComponent> ent, ref MeleeHitEvent args)
    {
        if (!_skillStorageQuery.TryComp(args.User, out var skillStorage))
            return;

        var passed = true;
        foreach (var reqSkill in ent.Comp.Skills)
        {
            if (!skillStorage.LearnedSkills.Contains(reqSkill))
            {
                passed = false;
                break;
            }
        }

        args.BonusDamage *= ent.Comp.DamageMultiplier;

        if (_net.IsClient)
            return;

        if (passed || !_random.Prob(ent.Comp.DropProbability))
            return;

        _hands.TryDrop(args.User, ent);
        _throwing.TryThrow(ent,  _random.NextAngle().ToWorldVec() * 2, 2f, args.User);
        _damageable.TryChangeDamage(args.User, args.BaseDamage);
        _popup.PopupEntity(Loc.GetString("cp14-skill-issue"), args.User, args.User, PopupType.Medium);
    }
}
