using Content.Shared._CP14.Procedural.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.GameTicking.Rules.Components;

[RegisterComponent, Access(typeof(CP14StationAdditionalModifierRule))]
public sealed partial class CP14StationAdditionalModifierRuleComponent : Component
{
    [DataField(required: true)]
    public List<ProtoId<CP14ProceduralModifierPrototype>> Modifiers;
}
