using Content.Shared._CP14.ModularCraft.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.ModularCraft.Modifiers;

public sealed partial class CP14ModularModifierAddComponents : CP14ModularCraftModifier
{
    [DataField]
    public ComponentRegistry? Components;

    [DataField]
    public List<EntProtoId>? CopyFromProto;

    [DataField]
    public bool Override = true;

    public override void Effect(EntityManager entManager, Entity<CP14ModularCraftStartPointComponent> start)
    {
        if (Components is not null)
            entManager.AddComponents(start, Components, Override);

        if (CopyFromProto is not null)
        {
            var prototypeManager = IoCManager.Resolve<IPrototypeManager>();
            foreach (var proto in CopyFromProto)
            {
                if (!prototypeManager.TryIndex(proto, out var indexed))
                    continue;

                entManager.AddComponents(start, indexed.Components, Override);
            }
        }
    }
}
