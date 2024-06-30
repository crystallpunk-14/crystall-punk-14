using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Farming.Prototypes;

/// <summary>
///
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
