using Content.Shared.Actions;

namespace Content.Shared._CP14.Roof;

/// <summary>
/// Marks an entity as a roof, allowing you to hide all roofs or show them back depending on different situations
/// </summary>
[RegisterComponent]
public sealed partial class CP14RoofComponent : Component
{
}

public sealed partial class CP14ToggleRoofVisibilityAction : InstantActionEvent
{
}
