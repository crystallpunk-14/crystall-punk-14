using Content.Server.Chat.Systems;
using Content.Shared._CP14.Magic;
using Content.Shared._CP14.Magic.Components;

namespace Content.Server._CP14.Magic;

public sealed partial class CP14MagicSystem : CP14SharedMagicSystem
{
    [Dependency] private readonly ChatSystem _chat = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CP14MagicEffectVerbalAspectComponent, CP14VerbalAspectSpeechEvent>(OnSpellSpoken);
    }

    private void OnSpellSpoken(Entity<CP14MagicEffectVerbalAspectComponent> ent, ref CP14VerbalAspectSpeechEvent args)
    {
        if (args.Performer is not null && args.Speech is not null)
            _chat.TrySendInGameICMessage(args.Performer.Value, args.Speech, InGameICChatType.Speak, false);
    }
}
