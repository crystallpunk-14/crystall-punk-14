using Robust.Shared.Audio;

namespace Content.Server._CP14.Alchemy;

[RegisterComponent, Access(typeof(CP14AlchemyExtractionSystem))]
public sealed partial class CP14PestleComponent : Component
{
    [DataField]
    public float Probability = 0.1f;

    [DataField]
    public SoundSpecifier HitSound = new SoundCollectionSpecifier("CP14Pestle", AudioParams.Default.WithVariation(0.2f));
}
