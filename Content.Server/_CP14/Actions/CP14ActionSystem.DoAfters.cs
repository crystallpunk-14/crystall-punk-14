using Content.Server.Chat.Systems;
using Content.Shared._CP14.Actions;
using Content.Shared._CP14.Actions.Components;
using Content.Shared.Actions.Events;

namespace Content.Server._CP14.Actions;

public sealed partial class CP14ActionSystem
{
    private void InitializeDoAfter()
    {
        SubscribeLocalEvent<CP14ActionSpeakingComponent, CP14ActionStartDoAfterEvent>(OnVerbalActionStarted);
        SubscribeLocalEvent<CP14ActionEmotingComponent, CP14ActionStartDoAfterEvent>(OnEmoteActionStarted);

        SubscribeLocalEvent<CP14ActionSpeakingComponent, ActionDoAfterEvent>(OnVerbalActionPerformed);
        SubscribeLocalEvent<CP14ActionEmotingComponent, ActionDoAfterEvent>(OnEmoteActionPerformed);
    }

    private void OnVerbalActionStarted(Entity<CP14ActionSpeakingComponent> ent, ref CP14ActionStartDoAfterEvent args)
    {
        var performer = GetEntity(args.Performer);
        _chat.TrySendInGameICMessage(performer, ent.Comp.StartSpeech, ent.Comp.Whisper ? InGameICChatType.Whisper : InGameICChatType.Speak, true);
    }

    private void OnEmoteActionStarted(Entity<CP14ActionEmotingComponent> ent, ref CP14ActionStartDoAfterEvent args)
    {
        var performer = GetEntity(args.Performer);
        _chat.TrySendInGameICMessage(performer, Loc.GetString(ent.Comp.EndEmote), InGameICChatType.Emote, true);
    }

    private void OnVerbalActionPerformed(Entity<CP14ActionSpeakingComponent> ent, ref ActionDoAfterEvent args)
    {
        var performer = GetEntity(args.Performer);
        _chat.TrySendInGameICMessage(performer, ent.Comp.EndSpeech, ent.Comp.Whisper ? InGameICChatType.Whisper : InGameICChatType.Speak, true);
    }

    private void OnEmoteActionPerformed(Entity<CP14ActionEmotingComponent> ent, ref ActionDoAfterEvent args)
    {
        var performer = GetEntity(args.Performer);
        _chat.TrySendInGameICMessage(performer, Loc.GetString(ent.Comp.EndEmote), InGameICChatType.Emote, true);
    }
}
