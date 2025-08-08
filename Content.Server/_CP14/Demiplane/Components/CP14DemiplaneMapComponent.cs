
namespace Content.Server._CP14.Demiplane.Components;

/// <summary>
///
/// </summary>
[RegisterComponent, Access(typeof(CP14DemiplaneSystem))]
public sealed partial class CP14DemiplaneMapComponent : Component
{
    [DataField]
    public Vector2i Position = Vector2i.Zero;
}
