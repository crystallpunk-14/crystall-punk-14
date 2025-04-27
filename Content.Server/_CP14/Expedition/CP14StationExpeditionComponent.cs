using Content.Shared._CP14.Demiplane.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Expedition;

[RegisterComponent]
public sealed partial class CP14StationExpeditionComponent : Component
{
    [DataField]
    public EntityUid? ExpeditionMap = null;

    [DataField(required: true)]
    public ProtoId<CP14DemiplaneLocationPrototype> Location = default!;

    [DataField(required: true)]
    public List<ProtoId<CP14DemiplaneModifierPrototype>> Modifiers = new();
}
