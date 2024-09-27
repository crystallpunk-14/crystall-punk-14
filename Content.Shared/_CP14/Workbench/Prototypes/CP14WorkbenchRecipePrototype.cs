/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using Content.Shared.Stacks;
using Content.Shared.Tag;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Workbench.Prototypes;

[Prototype("CP14Recipe")]
public sealed class CP14WorkbenchRecipePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public ProtoId<TagPrototype> Tag = default!;

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

    [DataField]
    public bool TryMergeSolutions = false;

    [DataField]
    public string Solution = "food";
}
