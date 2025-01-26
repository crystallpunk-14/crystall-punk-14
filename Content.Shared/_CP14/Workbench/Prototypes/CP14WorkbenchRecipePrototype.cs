/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using Content.Shared._CP14.Knowledge.Prototypes;
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
    public int ResultCount = 1;

    [DataField]
    public bool TryMergeSolutions = false;

    [DataField]
    public string Solution = "food";

    /// <summary>
    /// If the player does not have this knowledge, the recipe will not be displayed in the workbench.
    /// </summary>
    [DataField]
    public ProtoId<CP14KnowledgePrototype>? KnowledgeRequired;
}
