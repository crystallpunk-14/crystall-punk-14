
namespace Content.Server._CP14.Carrying;

/// <summary>
/// Stores the carrier of an entity being carried.
/// </summary>
[RegisterComponent]
public sealed partial class BeingCarriedComponent : Component
{
    public EntityUid Carrier = default!;
}
