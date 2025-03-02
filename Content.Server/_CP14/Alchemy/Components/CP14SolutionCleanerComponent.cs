using Content.Server._CP14.Alchemy.EntitySystems;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio;

namespace Content.Server._CP14.Alchemy.Components;

/// <summary>
/// gradually rounds down all reagents in the specified solution
/// </summary>
[RegisterComponent, Access(typeof(CP14SolutionCleanerSystem))]
public sealed partial class CP14SolutionCleanerComponent : Component
{
    [DataField(required: true)]
    public string Solution = string.Empty;

    /// <summary>
    /// the reagent will flow gradually by the specified number until it becomes normalized
    /// </summary>
    [DataField]
    public FixedPoint2 LeakageQuantity = 0.25f;

    [DataField]
    public TimeSpan UpdateFrequency = TimeSpan.FromSeconds(4f);

    [DataField]
    public TimeSpan NextUpdateTime = TimeSpan.Zero;

    [DataField]
    public SoundSpecifier NormalizeSound = new SoundPathSpecifier("/Audio/Ambience/Objects/drain.ogg")
    {
        Params = AudioParams.Default.WithVariation(0.03f)
    };
}

