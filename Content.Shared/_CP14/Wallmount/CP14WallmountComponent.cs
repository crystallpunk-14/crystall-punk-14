namespace Content.Shared._CP14.Wallmount;

/// <summary>
/// Automatically attaches the entity to the wall when it appears, or removes it
/// </summary>
[RegisterComponent, Access(typeof(CP14WallmountSystem))]
public sealed partial class CP14WallmountComponent : Component
{
}
