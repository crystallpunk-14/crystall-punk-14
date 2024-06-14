using Content.Server.Popups;
using Content.Shared._CP14.Skills;
using Content.Shared._CP14.Skills.Components;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._CP14.Skills;

public sealed partial class CP14SkillSystem : SharedCP14SkillSystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CP14SkillRequirementComponent, ExaminedEvent>(OnExamined);

        SubscribeLocalEvent<CP14SkillRequirementComponent, AttemptShootEvent>(OnAttemptShoot);
        SubscribeLocalEvent<CP14SkillRequirementComponent, MeleeHitEvent>(OnMeleeHit);
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
        text += Loc.GetString(!canUse ? "cp14-skill-examined-failed" : "cp14-skill-examined-success", ("color", !canUse ? "#c23030" : "#3fc488")) + "\n";

        args.PushMarkup(text);
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
                _popup.PopupEntity(Loc.GetString("cp14-skill-issue-drops", ("item", MetaData(requirement).EntityName)), args.User, args.User, PopupType.Large);
                _stun.TryParalyze(args.User, TimeSpan.FromSeconds(1.5f), true);
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

    private void OnAttemptShoot(Entity<CP14SkillRequirementComponent> requirement, ref AttemptShootEvent args)
    {
        if (!_random.Prob(requirement.Comp.FuckupChance))
            return;

        if (HasEnoughSkillToUse(args.User, requirement, out _))
            return;

        switch (_random.Next(2))
        {
            case 0:
                _popup.PopupEntity(Loc.GetString("cp14-skill-issue-drops", ("item", MetaData(requirement).EntityName)), args.User, args.User, PopupType.Large);
                _stun.TryParalyze(args.User, TimeSpan.FromSeconds(1.5f), true);
                args.Cancelled = true;
                break;
            case 1:
                _popup.PopupEntity(Loc.GetString("cp14-skill-issue-self-harm"), args.User, args.User, PopupType.Large);

                var damage = new DamageSpecifier();
                damage.DamageDict.Add("Blunt", _random.NextFloat(10));

                _damageable.TryChangeDamage(args.User, damage);
                break;
        }
    }
}
