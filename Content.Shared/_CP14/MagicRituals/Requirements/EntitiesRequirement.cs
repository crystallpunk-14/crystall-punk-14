using Content.Shared.Stacks;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicRituals.Requirements;

public sealed partial class EntitiesRequirement : CP14RitualRequirement
{
    [DataField]
    public float CheckRange = 1f;

    [DataField]
    public Dictionary<EntProtoId, int> RequiredEntities = new ();

    [DataField]
    public Dictionary<ProtoId<StackPrototype>, int> RequiredStack = new();

    [DataField]
    public bool ConsumeResource = false;

    /// <summary>
    /// Effect appearing in place of used entities
    /// </summary>
    [DataField]
    public EntProtoId? VisualEffect = "CP14DustEffect";

    public override void Check(EntityManager entManager)
    {
        throw new NotImplementedException();
    }
}
