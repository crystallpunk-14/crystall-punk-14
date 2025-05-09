using Content.Shared._CP14.Skill.Components;
using Content.Shared.Damage;
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
