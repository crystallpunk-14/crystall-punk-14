using Content.Shared.Destructible.Thresholds;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Demiplane.Prototypes;

/// <summary>
/// A prototype “Special Demiplane” that can appear on the demiplane map as the final room of a chain of demiplanes.
/// It is supposed to be used as a special demiplane with special modifiers, and mining resources and
/// equipment from here is the main task of adventurers
/// </summary>
[Prototype("cp14SpecialDemiplane")]
public sealed partial class CP14SpecialDemiplanePrototype : IPrototype
{
    [IdDataField] public string ID { get; } = default!;

    /// <summary>
    /// The difficulty levels at which this location can be generated.
    /// </summary>
    [DataField(required: true)]
    public MinMax Levels = new(1, 10);

    /// <summary>
    ///  The location config that will be used to generate the demiplane. May be null, and if so, the location will be generated using the default way.
    /// </summary>
    [DataField]
    public ProtoId<CP14DemiplaneLocationPrototype>? Location;

    /// <summary>
    /// Modifiers that will be automatically added to the demiplane when it is generated.
    /// </summary>
    [DataField]
    public List<ProtoId<CP14DemiplaneModifierPrototype>> Modifiers = new();
}
