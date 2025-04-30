using Content.Server._CP14.Demiplane;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking;
using Content.Server.RoundEnd;
using Content.Shared._CP14.MagicEnergy;
using Content.Shared.GameTicking;
using Content.Shared.CCVar;
using Robust.Shared.Configuration;
using Robust.Shared.Audio;
using Robust.Shared.Timing;

namespace Content.Server._CP14.RoundEnd;

public sealed partial class CP14RoundEndSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly CP14DemiplaneSystem _demiplane = default!;
    [Dependency] private readonly RoundEndSystem _roundEnd = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    private TimeSpan _roundEndMoment = TimeSpan.Zero;

    //TODO: вообще нет поддержки нескольких кристаллов на карте.
    public override void Initialize()
    {
        base.Initialize();

        InitCbt();

        SubscribeLocalEvent<CP14MagicContainerRoundFinisherComponent, CP14MagicEnergyLevelChangeEvent>(OnFinisherMagicEnergyLevelChange);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestartCleanup);
    }

    private void OnRoundRestartCleanup(RoundRestartCleanupEvent ev)
    {
        _roundEndMoment = TimeSpan.Zero; //Reset timer, so it cant affect next round in any case
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        UpdateCbt(frameTime);

        if (_roundEndMoment == TimeSpan.Zero)
            return;

        if (_roundEndMoment > _timing.CurTime)
            return;

        _demiplane.DeleteAllDemiplanes(safe: false);
        _chatSystem.DispatchGlobalAnnouncement(Loc.GetString("cp14-round-end"),
            announcementSound: new SoundPathSpecifier("/Audio/_CP14/Ambience/event_boom.ogg"));
        _roundEnd.EndRound();
    }

    private void OnFinisherMagicEnergyLevelChange(Entity<CP14MagicContainerRoundFinisherComponent> ent,
        ref CP14MagicEnergyLevelChangeEvent args)
    {
        //Alarm 50% magic energy left
        if (args.NewValue < args.OldValue && args.OldValue > args.MaxValue / 2 && args.NewValue <= args.MaxValue / 2)
        {
            _chatSystem.DispatchGlobalAnnouncement(
                Loc.GetString("cp14-round-end-monolith-50"),
                announcementSound: new SoundPathSpecifier("/Audio/_CP14/Ambience/event_boom.ogg"));
        }

        //We initiate round end timer
        if (_roundEndMoment == TimeSpan.Zero && args.NewValue == 0)
            StartRoundEndTimer();

        //Full charged - cancel roundEnd
        if (_roundEndMoment != TimeSpan.Zero && args.NewValue == args.MaxValue)
            CancelRoundEndTimer();
    }

    private void StartRoundEndTimer()
    {
        var roundEndDelay = TimeSpan.FromMinutes(_cfg.GetCVar(CCVars.CP14RoundEndMinutes));

        _roundEndMoment = _timing.CurTime + roundEndDelay;

        var time = roundEndDelay.Minutes;
        string unitsLocString;
        if (roundEndDelay.TotalSeconds < 60)
        {
            time = roundEndDelay.Seconds;
            unitsLocString = "eta-units-seconds";
        }
        else
        {
            time = roundEndDelay.Minutes;
            unitsLocString = "eta-units-minutes";
        }

        _chatSystem.DispatchGlobalAnnouncement(
            Loc.GetString(
                "cp14-round-end-monolith-discharged",
                ("time", time),
                ("units", Loc.GetString(unitsLocString))),
            announcementSound: new SoundPathSpecifier("/Audio/_CP14/Ambience/event_boom.ogg"));
    }

    private void CancelRoundEndTimer()
    {
        _roundEndMoment = TimeSpan.Zero;

        _chatSystem.DispatchGlobalAnnouncement(Loc.GetString("cp14-round-end-monolith-recharged"),
            announcementSound: new SoundPathSpecifier("/Audio/_CP14/Ambience/event_boom.ogg"));
    }
}
