using Content.Shared._CP14.Butchering.Components;
using Content.Shared.Storage;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Butchering.Prototypes;

/// <summary>
/// Prototype for a single butchering stage.
/// </summary>
[Prototype("CP14ButcherStage")]
public sealed class CP14ButcherStagePrototype : IPrototype
{
    [IdDataField] public string ID { get; private set; } = default!;

    /// <summary>Which tool/process is required to execute this stage.</summary>
    [DataField] public CP14ButcheringTool Tool = CP14ButcheringTool.Knife;

    /// <summary>Delay for this stage (seconds). Can be modified by held tool (e.g. SharpComponent multiplier) on server.</summary>
    [DataField] public float Delay = 8f;

    /// <summary>Items to spawn when this stage completes.</summary>
    [DataField] public List<EntitySpawnEntry> Spawned = new();

    /// <summary>
    /// Whether target must be dead. Set false for things like fish or plants,
    /// true for animals/people (default).
    /// </summary>
    [DataField] public bool RequireDead = true;

    /// <summary>Optional popup localization key to show on success.</summary>
    [DataField] public LocId? PopupOnSuccess;

    /// <summary>Optional sound on success.</summary>
    [DataField] public SoundSpecifier? SoundOnSuccess;
}
