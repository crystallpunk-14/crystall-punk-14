using Content.Shared._CP14.Religion.Prototypes;
using Content.Shared._CP14.Religion.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Religion.Components;

/// <summary>
///
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(CP14SharedReligionGodSystem))]
public sealed partial class CP14ReligionEntityComponent : Component
{
    [DataField(required: true)]
    public ProtoId<CP14ReligionPrototype>? Religion;

    public HashSet<EntityUid> PvsOverridedObservers = new();
    public ICommonSession? Session;
}
