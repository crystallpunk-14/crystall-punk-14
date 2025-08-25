using Content.Shared._CP14.Action.Components;
using Content.Shared.Actions.Events;
using Content.Shared.CombatMode.Pacification;

namespace Content.Shared._CP14.Action;

public abstract partial class CP14ActionSystem
{
    private void InitializeAttempts()
    {
        SubscribeLocalEvent<CP14ActionDangerousComponent, ActionAttemptEvent>(OnDangerousActionAttempt);
    }

    private void OnDangerousActionAttempt(Entity<CP14ActionDangerousComponent> ent, ref ActionAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        if (!HasComp<PacifiedComponent>(args.User))
            return;

        _popup.PopupClient(Loc.GetString("cp14-magic-spell-pacified"), args.User, args.User);
        args.Cancelled = true;
    }
}
