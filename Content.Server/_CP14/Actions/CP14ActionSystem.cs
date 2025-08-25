using Content.Server.Instruments;
using Content.Shared._CP14.Actions;
using Content.Shared._CP14.Actions.Components;
using Content.Shared.Actions.Events;
using Content.Shared.Instruments;

namespace Content.Server._CP14.Actions;

public sealed partial class CP14ActionSystem : CP14SharedActionSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14ActionRequiredMusicToolComponent, ActionAttemptEvent>(OnActionMusicAttempt);
    }

    private void OnActionMusicAttempt(Entity<CP14ActionRequiredMusicToolComponent> ent, ref ActionAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        var passed = false;
        var query = EntityQueryEnumerator<ActiveInstrumentComponent, InstrumentComponent>();
        while (query.MoveNext(out var uid, out var active, out var instrument))
        {
            if (!instrument.Playing)
                continue;

            if (Transform(uid).ParentUid != args.User)
                continue;

            passed = true;
            break;
        }

        if (passed)
            return;

        Popup.PopupClient(Loc.GetString("cp14-magic-music-aspect"), args.User, args.User);
        args.Cancelled = true;
    }
}
