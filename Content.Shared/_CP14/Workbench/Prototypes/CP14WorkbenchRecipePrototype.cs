using Content.Shared.Stacks;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Workbench.Prototypes;

[Prototype("CP14Recipe")]
public sealed class CP14WorkbenchRecipePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public TimeSpan CraftTime = TimeSpan.FromSeconds(1f);

    [DataField]
    public SoundSpecifier? OverrideCraftSound;

    [DataField]
    public Dictionary<EntProtoId, int> Entities = new();

    [DataField]
    public Dictionary<ProtoId<StackPrototype>, int> Stacks = new();

    [DataField(required: true)]
    public EntProtoId Result;
}
