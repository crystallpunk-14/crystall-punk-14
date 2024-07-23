using System.Numerics;
using Content.Server._CP14.Alchemy;
using Content.Server._CP14.MeleeWeapon;
using Content.Server._CP14.MeleeWeapon.EntitySystems;
using Content.Server.Popups;
using Content.Shared._CP14.Skills;
using Content.Shared._CP14.Skills.Components;
using Content.Shared.Chemistry.Components;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._CP14.Skills;

public sealed partial class CP14SkillSystem : SharedCP14SkillSystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CP14SkillRequirementComponent, ExaminedEvent>(OnExamined);

        SubscribeLocalEvent<CP14SkillRequirementComponent, GunShotEvent>(OnGunShot);
        SubscribeLocalEvent<CP14SkillRequirementComponent, MeleeHitEvent>(OnMeleeHit);
        SubscribeLocalEvent<CP14SkillRequirementComponent, CP14TrySkillIssueEvent>(OnSimpleSkillIssue);
        SubscribeLocalEvent<CP14SkillRequirementComponent, SharpingEvent>(OnSharpning);
        SubscribeLocalEvent<CP14SkillRequirementComponent, PestleGrindingEvent>(OnPestleGrinding);
    }

    private void OnExamined(Entity<CP14SkillRequirementComponent> requirement, ref ExaminedEvent args)
    {
        var text = string.Empty;
        text += "\n" + Loc.GetString("cp14-skill-label") + "\n";
        var canUse = HasEnoughSkillToUse(args.Examiner, requirement, out var missingSkills);

        text += Loc.GetString(requirement.Comp.NeedAll ? "cp14-skill-examined-need-all" : "cp14-skill-examined", ("item", MetaData(requirement).EntityName)) + "\n";
        foreach (var skill in requirement.Comp.RequiredSkills)
        {
            var name = _proto.Index(skill).Name;
            if (name == null)
                continue;

            var color = missingSkills.Contains(skill) ? "#c23030" : "#3fc488";
            text += Loc.GetString("cp14-skill-examined-skill", ("color", color), ("skill", Loc.GetString(name))) + "\n";

        }
        args.PushMarkup(text);
    }

    private void OnPestleGrinding(Entity<CP14SkillRequirementComponent> requirement, ref PestleGrindingEvent args)
    {
        if (!_random.Prob(requirement.Comp.FuckupChance))
            return;

        if (HasEnoughSkillToUse(args.User, requirement, out _))
            return;

        _popup.PopupEntity(Loc.GetString("cp14-skill-issue-push", ("item", MetaData(args.Target).EntityName)),
            args.User,
            args.User,
            PopupType.Large);
        _hands.TryDrop(args.User, args.Target);
        _throwing.TryThrow(args.Target, _random.NextAngle().ToWorldVec(), 1, args.User);
    }

    private void OnSharpning(Entity<CP14SkillRequirementComponent> requirement, ref SharpingEvent args)
    {
        if (!_random.Prob(requirement.Comp.FuckupChance))
            return;

        if (HasEnoughSkillToUse(args.User, requirement, out _))
            return;

        switch (_random.Next(2))
        {
            case 0:
                _popup.PopupEntity(Loc.GetString("cp14-skill-issue-sharp-weapon-harm", ("item", MetaData(args.Target).EntityName)), args.User, args.User, PopupType.Large);
                var weaponDamage = new DamageSpecifier();
                weaponDamage.DamageDict.Add("Blunt", _random.NextFloat(5));
                _damageable.TryChangeDamage(args.Target, weaponDamage);
                break;
            case 1:
                _popup.PopupEntity(Loc.GetString("cp14-skill-issue-sharp-self-harm"), args.User, args.User, PopupType.Large);

                var damage = new DamageSpecifier();
                if (TryComp<MeleeWeaponComponent>(requirement, out var melee))
                    damage = melee.Damage;
                else
                    damage.DamageDict.Add("Slash", _random.NextFloat(10));

                _damageable.TryChangeDamage(args.User, damage);
                break;
        }
    }

    private void OnSimpleSkillIssue(Entity<CP14SkillRequirementComponent> requirement, ref CP14TrySkillIssueEvent args)
    {
        if (!_random.Prob(requirement.Comp.FuckupChance))
            return;

        if (HasEnoughSkillToUse(args.User, requirement, out _))
            return;

        BasicSkillIssue(args.User, requirement);
    }

    private void OnMeleeHit(Entity<CP14SkillRequirementComponent> requirement, ref MeleeHitEvent args)
    {
        if (!_random.Prob(requirement.Comp.FuckupChance))
            return;

        if (HasEnoughSkillToUse(args.User, requirement, out _))
            return;

        switch (_random.Next(2))
        {
            case 0:
                BasicSkillIssue(args.User, requirement);
                break;
            case 1:
                _popup.PopupEntity(Loc.GetString("cp14-skill-issue-self-harm"), args.User, args.User, PopupType.Large);

                var damage = new DamageSpecifier();
                if (TryComp<MeleeWeaponComponent>(requirement, out var melee))
                    damage = melee.Damage;
                else
                    damage.DamageDict.Add("Blunt", _random.NextFloat(10));

                _damageable.TryChangeDamage(args.User, damage);
                break;
        }
    }

    private void OnGunShot(Entity<CP14SkillRequirementComponent> requirement, ref GunShotEvent args)
    {
        if (!_random.Prob(requirement.Comp.FuckupChance))
            return;

        if (HasEnoughSkillToUse(args.User, requirement, out _))
            return;

        _popup.PopupEntity(Loc.GetString("cp14-skill-issue-recoil"), args.User, args.User, PopupType.Large);
        _hands.TryDrop(args.User, requirement);
        _throwing.TryThrow(requirement, _random.NextAngle().ToWorldVec(), _random.NextFloat(5), args.User);

        var damage = new DamageSpecifier();
        damage.DamageDict.Add("Blunt", _random.NextFloat(10));
        _damageable.TryChangeDamage(args.User, damage);
    }

    private void BasicSkillIssue(EntityUid user, EntityUid item, float throwPower = 1)
    {
        _popup.PopupEntity(Loc.GetString("cp14-skill-issue-drops", ("item", MetaData(item).EntityName)), user, user, PopupType.Large);
        _hands.TryDrop(user, item);
        _throwing.TryThrow(item, _random.NextAngle().ToWorldVec(), throwPower, user);
    }
}
