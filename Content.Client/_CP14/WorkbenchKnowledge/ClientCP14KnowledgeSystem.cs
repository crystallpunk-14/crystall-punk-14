using Content.Shared._CP14.Workbench.Prototypes;
using Content.Shared._CP14.WorkbenchKnowledge;
using Robust.Client.Player;
using Robust.Shared.Prototypes;

namespace Content.Client._CP14.WorkbenchKnowledge;

public sealed class ClientCP14KnowledgeSystem : SharedCP14WorkbenchKnowledgeSystem
{
    [Dependency] private readonly IPlayerManager _players = default!;


    public override void Initialize()
    {
        base.Initialize();

    }

    public readonly record struct KnowledgeData(
        EntityUid Entity,
        HashSet<ProtoId<CP14WorkbenchRecipePrototype>> AllKnowledge
    );
}
