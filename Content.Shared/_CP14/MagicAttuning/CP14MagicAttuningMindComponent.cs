using Robust.Shared.GameStates;

namespace Content.Shared._CP14.MagicAttuning;

/// <summary>
/// A mind that can focus on objects
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(CP14SharedMagicAttuningSystem))]
public sealed partial class CP14MagicAttuningMindComponent : Component
{
    [DataField]
    public int MaxAttuning = 3;
    /// <summary>
    /// The entities that this being is focused on
    /// </summary>
    [DataField]
    public List<EntityUid> AttunedTo = new();

    /// <summary>
    /// cheat: if added to an entity with MindContainer, automatically copied to the mind, removing it from the body. This is to make it easy to add the component to prototype creatures.
    /// </summary>
    [DataField]
    public bool AutoCopyToMind = false;
}
