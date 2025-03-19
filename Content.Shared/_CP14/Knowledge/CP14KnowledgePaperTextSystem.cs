using System.Text;
using Content.Shared._CP14.Knowledge.Components;
using Content.Shared.Paper;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Knowledge;

public sealed class CP14KnowledgePaperTextSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly PaperSystem _paper = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CP14KnowledgePaperTextComponent, MapInitEvent>(OnPaperMapInit);
    }

    private void OnPaperMapInit(Entity<CP14KnowledgePaperTextComponent> ent, ref MapInitEvent args)
    {
        if (!TryComp<PaperComponent>(ent, out var paperComponent))
            return;

        var paper = new Entity<PaperComponent>(ent, paperComponent);

        if (!TryComp<CP14KnowledgeLearningSourceComponent>(ent, out var knowledge))
            return;

        if (knowledge.Knowledge.Count == 0)
            return;

        var content = GenerateText(knowledge);

        _paper.SetContent(paper, content);
        paper.Comp.EditingDisabled = true;

        // Yes we need to do synchronization after such changes,
        // yes predict, but we need to
        Dirty(paper);
    }

    private string GenerateText(CP14KnowledgeLearningSourceComponent source)
    {
        var stringBuilder = new StringBuilder();

        // Header
        stringBuilder.Append(Loc.GetString("cp14-knowledge-book-pre-text"));

        // Main body
        foreach (var prototypeId in source.Knowledge)
        {
            if (!_prototype.TryIndex(prototypeId, out var indexedKnowledge))
                continue;

            stringBuilder.Append($"\n{Loc.GetString(indexedKnowledge.Desc)}");
        }

        // Footer
        stringBuilder.Append($"\n\n{Loc.GetString("cp14-knowledge-book-post-text")}");

        return stringBuilder.ToString();
    }
}
