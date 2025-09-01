using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Butchering.Components;

/// <summary>
/// Which tool/process is valid for the stage.
/// Mirrors vanilla enum to stay upstream-compatible.
/// </summary>
[Serializable, NetSerializable]
public enum CP14ButcheringTool : byte
{
    Knife,
    Spike,
    Gibber
}
