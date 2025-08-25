using Content.Shared._CP14.Action.Components;
using Content.Shared.Actions.Components;
using Content.Shared.Actions.Events;
using Content.Shared.CombatMode.Pacification;

namespace Content.Shared._CP14.Action;

public sealed partial class CP14ActionSystem
{
    private EntityQuery<TargetActionComponent> _targetQuery;
    private EntityQuery<WorldTargetActionComponent> _worldTargetQuery;
    private EntityQuery<EntityTargetActionComponent> _entityTargetQuery;

    private void InitializeAttempts()
    {
        var _targetQuery = GetEntityQuery<TargetActionComponent>();
        var _worldTargetQuery = GetEntityQuery<WorldTargetActionComponent>();
        var _entityTargetQuery = GetEntityQuery<EntityTargetActionComponent>();

        SubscribeLocalEvent<CP14ActionDangerousComponent, ActionAttemptEvent>(OnDangerousActionAttempt);
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
