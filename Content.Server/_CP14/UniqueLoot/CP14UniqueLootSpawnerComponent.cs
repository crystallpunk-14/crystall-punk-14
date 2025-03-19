using Content.Shared.Tag;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.UniqueLoot;

[RegisterComponent]
public sealed partial class CP14UniqueLootSpawnerComponent : Component
{
    /// <summary>
    /// Used to filter which types of unique loot can be generated. You can leave null to disable filtering.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<TagPrototype>? Tag;
}
