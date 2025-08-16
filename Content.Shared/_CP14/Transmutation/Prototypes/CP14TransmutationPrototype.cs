using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Transmutation.Prototypes;

[Prototype("cp14Transmutation")]
public sealed partial class CP14TransmutationPrototype : IPrototype
{
    [IdDataField] public string ID { get; private set; } = default!;

    /// <summary>
    /// Clientsude entity, that spawns when transmutations appear
    /// </summary>
    [DataField("vfx")]
    public EntProtoId? VFX;

    [DataField]
    public SoundSpecifier? Sound;
}
