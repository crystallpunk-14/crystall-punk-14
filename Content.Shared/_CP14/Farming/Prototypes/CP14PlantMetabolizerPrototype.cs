using Content.Shared._CP14.Farming.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Farming.Prototypes;

/// <summary>
/// Allows the plant to drink chemicals from the soil. The effect of the drank reagents depends on the selected metabolizer.
/// </summary>
[Prototype("CP14PlantMetabolizer")]
public sealed partial class CP14PlantMetabolizerPrototype : IPrototype
{
    [ViewVariables]
    [IdDataField]
    public string ID { get; private set; } = string.Empty;

    [DataField]
    public Dictionary<ProtoId<ReagentPrototype>, HashSet<CP14MetabolizerEffect>> Metabolization = new();
}

[ImplicitDataDefinitionForInheritors]
[MeansImplicitUse]
public abstract partial class CP14MetabolizerEffect
{
    public abstract void Effect(Entity<CP14PlantComponent> plant, FixedPoint2 amount, EntityManager entityManager);
}
