using Content.Server.Chat.Systems;
using Content.Shared._CP14.Actions;
using Content.Shared._CP14.Actions.Components;
using Content.Shared.Actions.Events;

namespace Content.Server._CP14.Actions;

public sealed partial class CP14ActionSystem
{
    private void InitializePerformed()
    {
        SubscribeLocalEvent<CP14ActionSpeakingComponent, CP14ActionStartDoAfterEvent>(OnVerbalActionStarted);
        SubscribeLocalEvent<CP14ActionEmotingComponent, CP14ActionStartDoAfterEvent>(OnEmoteActionStarted);

        SubscribeLocalEvent<CP14ActionSpeakingComponent, ActionPerformedEvent>(OnVerbalActionPerformed);
        SubscribeLocalEvent<CP14ActionEmotingComponent, ActionPerformedEvent>(OnEmoteActionPerformed);
    }

    private void OnVerbalActionStarted(Entity<CP14ActionSpeakingComponent> ent, ref CP14ActionStartDoAfterEvent args)
    {
        var performer = GetEntity(args.Performer);
        _chat.TrySendInGameICMessage(performer, ent.Comp.StartSpeech, ent.Comp.Whisper ? InGameICChatType.Whisper : InGameICChatType.Speak, true);
    }

    private void OnEmoteActionStarted(Entity<CP14ActionEmotingComponent> ent, ref CP14ActionStartDoAfterEvent args)
    {
        var performer = GetEntity(args.Performer);
        _chat.TrySendInGameICMessage(performer, ent.Comp.EndEmote, InGameICChatType.Emote, true);
    }

    private void OnVerbalActionPerformed(Entity<CP14ActionSpeakingComponent> ent, ref ActionPerformedEvent args)
    {
        _chat.TrySendInGameICMessage(args.Performer, ent.Comp.EndSpeech, ent.Comp.Whisper ? InGameICChatType.Whisper : InGameICChatType.Speak, true);
    }

    private void OnEmoteActionPerformed(Entity<CP14ActionEmotingComponent> ent, ref ActionPerformedEvent args)
    {
        _chat.TrySendInGameICMessage(args.Performer, ent.Comp.EndEmote, InGameICChatType.Emote, true);
    }
}
