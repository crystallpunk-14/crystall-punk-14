using Content.Server.Chat.Managers;
using Content.Server.Mind;
using Content.Shared._CP14.Workbench.Prototypes;
using Content.Shared._CP14.WorkbenchKnowledge;
using Content.Shared._CP14.WorkbenchKnowledge.Components;
using Content.Shared.Administration.Logs;
using Content.Shared.Chat;
using Content.Shared.Database;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.WorkbenchKnowledge;

public sealed class CP14WorkbenchKnowledgeSystem : SharedCP14WorkbenchKnowledgeSystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;

    public override bool TryAdd(Entity<CP14WorkbenchKnowledgeStorageComponent?> entity, ProtoId<CP14WorkbenchRecipePrototype> recipeId, bool force = false, bool silent = false)
    {
        if (!Resolve(entity, ref entity.Comp, false))
            return false;

        if (HasKnowledge(entity, recipeId))
            return false;

        if (!_proto.TryIndex(recipeId, out var knowledge))
            return false;

        if (!entity.Comp.Recipes.Add(recipeId))
            return false;

        _adminLogger.Add(LogType.Mind, LogImpact.Medium, $"{EntityManager.ToPrettyString(entity):player} learned new workbench recipe: {Loc.GetString(knowledge.ID)}");
        Dirty(entity);

        if (silent)
            return true;

        if (!_mind.TryGetMind(entity, out _, out var mindComponent) || mindComponent.Session is null)
            return false;

        if (!_proto.TryIndex(knowledge.Result, out var resultProto))
            return false;

        var message = Loc.GetString("cp14-learned-new-knowledge", ("name", resultProto.Name));
        var wrappedMessage2 = Loc.GetString("chat-manager-server-wrap-message", ("message", message));

        _chat.ChatMessageToOne(
            ChatChannel.Server,
            message,
            wrappedMessage2,
            default,
            false,
            mindComponent.Session.Channel);

        return true;
    }

    public override bool TryRemove(Entity<CP14WorkbenchKnowledgeStorageComponent?> entity, ProtoId<CP14WorkbenchRecipePrototype> proto, bool silent = false)
    {
        if (!Resolve(entity, ref entity.Comp, false))
            return false;

        if (!_proto.TryIndex(proto, out var indexedKnowledge))
            return false;

        if (!entity.Comp.Recipes.Remove(proto))
            return false;

        _adminLogger.Add(LogType.Mind, LogImpact.Medium, $"{EntityManager.ToPrettyString(entity):player} forgot workbench recipe: {Loc.GetString(indexedKnowledge.ID)}");
        Dirty(entity);

        if (silent)
            return true;

        if (!_mind.TryGetMind(entity, out _, out var mindComp) || mindComp.Session is null)
            return true;


        if (!_proto.TryIndex(indexedKnowledge.Result, out var resultProto))
            return false;

        var message = Loc.GetString("cp14-forgot-knowledge", ("name", resultProto.Name));
        var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", message));

        _chat.ChatMessageToOne(
            ChatChannel.Server,
            message,
            wrappedMessage,
            default,
            false,
            mindComp.Session.Channel);

        return true;
    }
}
