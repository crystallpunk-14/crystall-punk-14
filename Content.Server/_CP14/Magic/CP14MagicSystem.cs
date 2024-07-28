using Content.Server.Chat.Systems;
using Content.Shared._CP14.Magic;
using Content.Shared._CP14.Magic.Components;
using Content.Shared._CP14.Magic.Events;

namespace Content.Server._CP14.Magic;

public sealed partial class CP14MagicSystem : CP14SharedMagicSystem
{
    [Dependency] private readonly ChatSystem _chat = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CP14MagicEffectVerbalAspectComponent, CP14VerbalAspectSpeechEvent>(OnSpellSpoken);

        SubscribeLocalEvent<CP14MagicEffectCastingVisualComponent, CP14StartCastMagicEffectEvent>(OnSpawnMagicVisualEffect);
        SubscribeLocalEvent<CP14MagicEffectCastingVisualComponent, CP14StopCastMagicEffectEvent>(OnDespawnMagicVisualEffect);
    }

    private void OnSpellSpoken(Entity<CP14MagicEffectVerbalAspectComponent> ent, ref CP14VerbalAspectSpeechEvent args)
    {
        if (args.Performer is not null && args.Speech is not null)
            _chat.TrySendInGameICMessage(args.Performer.Value, args.Speech, InGameICChatType.Speak, false);
    }

    private void OnSpawnMagicVisualEffect(Entity<CP14MagicEffectCastingVisualComponent> ent, ref CP14StartCastMagicEffectEvent args)
    {
        var vfx = SpawnAttachedTo(ent.Comp.Proto, Transform(args.Performer).Coordinates);
        ent.Comp.SpawnedEntity = vfx;
    }

    private void OnDespawnMagicVisualEffect(Entity<CP14MagicEffectCastingVisualComponent> ent, ref CP14StopCastMagicEffectEvent args)
    {
        QueueDel(ent.Comp.SpawnedEntity);
        ent.Comp.SpawnedEntity = null;
    }
}
