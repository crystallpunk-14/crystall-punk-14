using Content.Shared.Actions;

namespace Content.Shared._CP14.NightVision;

public abstract class CP14SharedNightVisionSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14NightVisionComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<CP14NightVisionComponent, ComponentRemove>(OnRemove);
    }

    private void OnMapInit(Entity<CP14NightVisionComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent, ref ent.Comp.ActionEntity, ent.Comp.ActionPrototype);
    }

    protected virtual void OnRemove(Entity<CP14NightVisionComponent> ent, ref ComponentRemove args)
    {
        _actions.RemoveAction(ent, ent.Comp.ActionEntity);
    }
}

public sealed partial class CP14ToggleNightVisionEvent : InstantActionEvent { }
