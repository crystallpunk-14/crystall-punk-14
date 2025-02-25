using Content.Server.Chat.Systems;
using Robust.Shared.Random;

namespace Content.Server._CP14.Demiplane;

public sealed partial class CP14DemiplaneSystem
{
    private void InitEchoes()
    {
        SubscribeLocalEvent<EntitySpokeEvent>(OnSpeak);
    }

    private void OnSpeak(EntitySpokeEvent ev)
    {
        var map = Transform(ev.Source).MapUid;

        if (!_demiplaneQuery.TryComp(map, out var demiplane))
            return;

        //Get random exit, and send message there
        if (demiplane.ExitPoints.Count == 0)
            return;
        var exit = _random.Pick(demiplane.ExitPoints);

        _chat.TrySendInGameICMessage(exit,
            ev.Message,
            InGameICChatType.Whisper,
            ChatTransmitRange.Normal,
            true,
            checkRadioPrefix: false,
            nameOverride: Loc.GetString("cp14-demiplane-echoes"),
            ignoreActionBlocker: false);
    }
}
