using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Placeable;
using Content.Shared.Tag;
using Content.Shared.Throwing;
using Robust.Shared.Physics.Events;

namespace Content.Shared._CP14.ItemPlacerParenting;

/// <summary>
///
/// </summary>
public sealed class CP14ItemPlacerAutoParentSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14ItemPlacerAutoParentComponent, ItemPlacedEvent>(OnItemPlaced);
        SubscribeLocalEvent<CP14ItemPlacerAutoParentComponent, ThrownEvent>(OnThrown);
        SubscribeLocalEvent<CP14ItemPlacerAutoParentComponent, EndCollideEvent>(OnEndCollide);
    }

    private void OnEndCollide(Entity<CP14ItemPlacerAutoParentComponent> ent, ref EndCollideEvent args)
    {
        if (!TryComp<ItemPlacerComponent>(ent, out var itemPlacer))
            return;

        Detach(args.OtherEntity, ent);
    }

    private void Detach(EntityUid item, EntityUid parent)
    {
        if (Transform(item).ParentUid == parent)
            _transform.SetParent(item, Transform(parent).ParentUid);
    }

    private void OnThrown(Entity<CP14ItemPlacerAutoParentComponent> ent, ref ThrownEvent args)
    {
        if (!TryComp<ItemPlacerComponent>(ent, out var itemPlacer))
            return;

        foreach (var placed in itemPlacer.PlacedEntities)
        {
            Detach(placed, ent);
        }
    }

    private void OnItemPlaced(Entity<CP14ItemPlacerAutoParentComponent> ent, ref ItemPlacedEvent args)
    {
        if (HasComp<CP14ItemPlacerParentedComponent>(ent))
            return;

        if (!TryComp<ItemPlacerComponent>(ent, out var itemPlacer))
            return;

        _transform.SetParent(args.OtherEntity, ent);
        AddComp<CP14ItemPlacerParentedComponent>(ent);
    }

    private void Detach(EntityUid target)
    {

    }
}
