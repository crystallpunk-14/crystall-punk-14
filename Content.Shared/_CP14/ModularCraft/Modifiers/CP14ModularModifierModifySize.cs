using System.Linq;
using Content.Shared._CP14.ModularCraft.Components;
using Content.Shared.Item;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.ModularCraft.Modifiers;

public sealed partial class CP14ModularModifierModifySize : CP14ModularCraftModifier
{
    [DataField]
    public ProtoId<ItemSizePrototype>? NewSize;

    /// <summary>
    /// Only works if the item has 1 shape. Increases or decreases it size.
    /// </summary>
    [DataField]
    public Vector2i? AdjustShape;

    public override void Effect(EntityManager entManager, Entity<CP14ModularCraftStartPointComponent> start)
    {
        if (!entManager.TryGetComponent<ItemComponent>(start, out var itemComp) || itemComp.Shape is null)
            return;

        var itemSystem = entManager.System<SharedItemSystem>();

        if (NewSize is not null)
            itemSystem.SetSize(start, NewSize.Value);

        if (AdjustShape is not null && itemSystem.GetItemShape((start, itemComp)).Count == 1)
        {
            var box = itemComp.Shape.First();
            box.Right += AdjustShape.Value.X;
            box.Bottom += AdjustShape.Value.Y;
            itemSystem.SetShape(start, new List<Box2i>{box});
        }
    }
}
