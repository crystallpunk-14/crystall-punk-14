using System.Linq;
using Content.Shared._CP14.Action.Components;
using Content.Shared.Actions.Components;
using Content.Shared.Actions.Events;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;

namespace Content.Shared._CP14.Action;

public sealed partial class CP14ActionSystem
{
    private void InitializeAttempts()
    {
        SubscribeLocalEvent<CP14ActionDangerousComponent, ActionAttemptEvent>(OnDangerousActionAttempt);
        SubscribeLocalEvent<CP14ActionTargetMobStatusRequiredComponent, ActionValidateEvent>(OnTargetMobStatusRequiredValidate);
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
}
