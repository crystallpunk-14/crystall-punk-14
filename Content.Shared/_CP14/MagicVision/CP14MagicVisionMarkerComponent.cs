using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.MagicVision;

/// <summary>
/// Controls the visibility of this entity to the client, based on the length of time it has existed and the client's ability to see the magic
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true), AutoGenerateComponentPause]
public sealed partial class CP14MagicVisionMarkerComponent : Component
{
    [DataField, AutoPausedField, AutoNetworkedField]
    public TimeSpan SpawnTime = TimeSpan.Zero;

    [DataField, AutoPausedField, AutoNetworkedField]
    public TimeSpan EndTime = TimeSpan.Zero;

    [DataField, AutoNetworkedField]
    public SpriteSpecifier? Icon;
}
