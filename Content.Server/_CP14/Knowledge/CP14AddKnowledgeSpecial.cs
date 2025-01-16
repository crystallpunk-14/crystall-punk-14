using Content.Shared._CP14.Knowledge;
using Content.Shared._CP14.Knowledge.Prototypes;
using Content.Shared.Roles;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Knowledge;

/// <summary>
/// a component that can be hung on an entity to immediately teach it some skills
/// </summary>
[UsedImplicitly]
public sealed partial class CP14AddKnowledgeSpecial : JobSpecial
{
    [DataField(required: true), ViewVariables(VVAccess.ReadOnly)]
    public List<ProtoId<CP14KnowledgePrototype>> Knowledge = new();

    public override void AfterEquip(EntityUid mob)
    {
        var entMan = IoCManager.Resolve<IEntityManager>();
        var knowledgeSystem = entMan.System<CP14KnowledgeSystem>();
        foreach (var knowledge in Knowledge)
        {
            knowledgeSystem.TryLearnKnowledge(mob, knowledge, true, true);
        }
    }
}
