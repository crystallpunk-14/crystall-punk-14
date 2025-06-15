using Content.Shared._CP14.Religion.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared._CP14.Religion.Components;

/// <summary>
/// Limits the vision of entities, allowing them to see only areas within a radius around observers of their religion.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(CP14SharedReligionGodSystem))]
public sealed partial class CP14ReligionVisionComponent : Component
{
    [DataField]
    public Vector3 ShaderColor = new (1f, 1f, 1f);
}
