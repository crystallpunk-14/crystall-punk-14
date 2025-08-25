using System.Linq;
using Content.Shared._CP14.Action.Components;
using Content.Shared._CP14.MagicSpell.Components;
using Content.Shared._CP14.Skill.Components;
using Content.Shared.Actions.Events;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;

namespace Content.Shared._CP14.Action;

public sealed partial class CP14ActionSystem
{
    [Dependency] private readonly SharedHandsSystem _hand = default!;

    private void InitializeAttempts()
    {
        SubscribeLocalEvent<CP14ActionFreeHandsRequiredComponent, ActionAttemptEvent>(OnSomaticActionAttempt);

        SubscribeLocalEvent<CP14ActionDangerousComponent, ActionAttemptEvent>(OnDangerousActionAttempt);
        SubscribeLocalEvent<CP14ActionTargetMobStatusRequiredComponent, ActionValidateEvent>(OnTargetMobStatusRequiredValidate);
        SubscribeLocalEvent<CP14ActionSkillPointCostComponent, ActionAttemptEvent>(OnSkillPointActionAttempt);
    }

    private void OnSomaticActionAttempt(Entity<CP14ActionFreeHandsRequiredComponent> ent, ref ActionAttemptEvent args)
    {
        if (TryComp<HandsComponent>(args.User, out var hands) || hands is not null)
        {
            if (_hand.CountFreeableHands((args.User, hands)) >= ent.Comp.FreeHandRequired)
                return;
        }

        _popup.PopupClient(Loc.GetString("cp14-magic-spell-need-somatic-component"), args.User, args.User);
        args.Cancelled = true;
    }

    private void OnTargetMobStatusRequiredValidate(Entity<CP14ActionTargetMobStatusRequiredComponent> ent, ref ActionValidateEvent args)
    {
        var target = GetEntity(args.Input.EntityTarget);

        if (!TryComp<MobStateComponent>(target, out var mobStateComp))
        {
            _popup.PopupClient(Loc.GetString("cp14-magic-spell-target-not-mob"), args.User, args.User);
            args.Invalid = true;
            return;
        }

        if (!ent.Comp.AllowedStates.Contains(mobStateComp.CurrentState))
        {
            var states = string.Join(", ",
                ent.Comp.AllowedStates.Select(state => state switch
                {
                    MobState.Alive => Loc.GetString("cp14-magic-spell-target-mob-state-live"),
                    MobState.Dead => Loc.GetString("cp14-magic-spell-target-mob-state-dead"),
                    MobState.Critical => Loc.GetString("cp14-magic-spell-target-mob-state-critical")
                }));

            _popup.PopupClient(Loc.GetString("cp14-magic-spell-target-mob-state", ("state", states)), args.User, args.User);
            args.Invalid = true;
        }
    }

    private void OnDangerousActionAttempt(Entity<CP14ActionDangerousComponent> ent, ref ActionAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        if (HasComp<PacifiedComponent>(args.User))
        {
            _popup.PopupClient(Loc.GetString("cp14-magic-spell-pacified"), args.User, args.User);
            args.Cancelled = true;
        }
    }

    private void OnSkillPointActionAttempt(Entity<CP14ActionSkillPointCostComponent> ent, ref ActionAttemptEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.SkillPoint, out var indexedSkillPoint) || ent.Comp.SkillPoint is null)
            return;

        if (!TryComp<CP14SkillStorageComponent>(args.User, out var skillStorage))
        {
            _popup.PopupClient(Loc.GetString("cp14-magic-spell-skillpoint-not-enough", ("name", Loc.GetString(indexedSkillPoint.Name)), ("count", ent.Comp.Count)), args.User, args.User);
            args.Cancelled = true;
            return;
        }

        var points = skillStorage.SkillPoints;
        if (points.TryGetValue(ent.Comp.SkillPoint.Value, out var currentPoints))
        {
            var freePoints = currentPoints.Max - currentPoints.Sum;

            if (freePoints < ent.Comp.Count)
            {
                var d = ent.Comp.Count - freePoints;

                _popup.PopupClient(Loc.GetString("cp14-magic-spell-skillpoint-not-enough",
                    ("name", Loc.GetString(indexedSkillPoint.Name)),
                    ("count", d)),
                    args.User,
                    args.User);
                args.Cancelled = true;
            }
        }
    }
}
