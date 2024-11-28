using Content.Shared._CP14.ModularCraft;
using Content.Shared._CP14.ModularCraft.Components;
using Content.Shared._CP14.ModularCraft.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.ModularCraft.Modifiers;

public sealed partial class Inherit : CP14ModularCraftModifier
{
    [DataField(required: true)]
    public List<ProtoId<CP14ModularCraftPartPrototype>> CopyFrom = new();

    public override void Effect(EntityManager entManager, Entity<CP14ModularCraftStartPointComponent> start, Entity<CP14ModularCraftPartComponent>? part)
    {
        var prototypeManager = IoCManager.Resolve<IPrototypeManager>();

        foreach (var copy in CopyFrom)
        {
            foreach (var modifier in prototypeManager.Index(copy).Modifiers)
            {
                modifier.Effect(entManager, start, part);
            }
        }
    }
}
