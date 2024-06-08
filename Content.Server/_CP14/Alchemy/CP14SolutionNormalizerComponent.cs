
using Content.Shared.FixedPoint;
using Robust.Shared.Audio;

namespace Content.Server._CP14.Alchemy;

/// <summary>
/// gradually rounds down all reagents in the specified solution
/// </summary>
[RegisterComponent, Access(typeof(CP14SolutionNormalizerSystem))]
public sealed partial class CP14SolutionNormalizerComponent : Component
{
    [DataField(required: true)]
    public string Solution = string.Empty;

    /// <summary>
    /// will round down any reagent in solution until it is divisible by this value
    /// </summary>
    [DataField]
    public float Factor = 0.25f;

    /// <summary>
    /// the reagent will flow gradually by the specified number until it becomes normalized
    /// </summary>
    [DataField]
    public FixedPoint2 LeakageQuantity = 0.05f;

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

