using Content.Server.Popups;
using Content.Shared._CP14.Skills;
using Content.Shared._CP14.Skills.Components;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Random;

namespace Content.Server._CP14.Skills;

public sealed partial class CP14SkillSystem : SharedCP14SkillSystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CP14SkillRequirementComponent, ExaminedEvent>(OnExamined);

        SubscribeLocalEvent<CP14SkillRequirementComponent, AttemptMeleeEvent>(OnAttemptMelee);
        SubscribeLocalEvent<CP14SkillRequirementComponent, AttemptShootEvent>(OnAttemptShoot);
    }

    private void OnExamined(Entity<CP14SkillRequirementComponent> requirement, ref ExaminedEvent args)
    {

    }

    private void OnAttemptShoot(Entity<CP14SkillRequirementComponent> requirement, ref AttemptShootEvent args)
    {
        if (!_random.Prob(requirement.Comp.FuckupChance))
            return;

    }

    private void OnAttemptMelee(Entity<CP14SkillRequirementComponent> requirement, ref AttemptMeleeEvent args)
    {
        if (!_random.Prob(requirement.Comp.FuckupChance))
            return;

        switch (_random.Next(2))
        {
            case 0:
                _popup.PopupEntity(Loc.GetString("cp14-skill-issue-melee-1", ("item", MetaData(requirement).EntityName)), args.User, args.User, PopupType.Large);
                _stun.TryParalyze(args.User, TimeSpan.FromSeconds(1.5f), true);
                break;
            case 1:
                _popup.PopupEntity(Loc.GetString("cp14-skill-issue-melee-2"), args.User, args.User, PopupType.Large);
                var damage = new DamageSpecifier();
                damage.DamageDict.Add("Blunt", _random.NextFloat(10));
                _damageable.TryChangeDamage(args.User, damage);
                break;
        }
    }
}
