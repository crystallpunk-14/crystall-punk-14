using Content.Server._CP14.Demiplane;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking;
using Content.Shared._CP14.MagicEnergy;
using Robust.Shared.Audio;
using Robust.Shared.Timing;

namespace Content.Server._CP14.RoundEnd;

public sealed partial class CP14RoundEndSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly CP14DemiplaneSystem _demiplane = default!;

    private readonly TimeSpan _roundEndDelay = TimeSpan.FromMinutes(15);
    private TimeSpan _roundEndMoment = TimeSpan.Zero;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14MagicContainerRoundFinisherComponent, CP14MagicEnergyLevelChangeEvent>(OnFinisherMagicEnergyLevelChange);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_roundEndMoment == TimeSpan.Zero)
            return;

        if (_roundEndMoment > _timing.CurTime)
            return;

        EndRound();
    }

    public bool IsMonolithActive()
    {
        if (_gameTicker.RunLevel != GameRunLevel.InRound)
            return false;

        if (_roundEndMoment != TimeSpan.Zero)
            return false;

        return true;
    }

    private void OnFinisherMagicEnergyLevelChange(Entity<CP14MagicContainerRoundFinisherComponent> ent,
        ref CP14MagicEnergyLevelChangeEvent args)
    {
        //We initiate round end timer
        if (_roundEndMoment == TimeSpan.Zero && args.NewValue == 0)
        {
            StartRoundEndTimer();
        }

        if (_roundEndMoment != TimeSpan.Zero && args.NewValue == args.MaxValue)
        {
            EndRoundEndTimer();
        }
    }

    private void StartRoundEndTimer()
    {
        _roundEndMoment = _timing.CurTime + _roundEndDelay;

        var time = _roundEndDelay.Minutes;
        string unitsLocString;
        if (_roundEndDelay.TotalSeconds < 60)
        {
            time = _roundEndDelay.Seconds;
            unitsLocString = "eta-units-seconds";
        }
        else
        {
            time = _roundEndDelay.Minutes;
            unitsLocString = "eta-units-minutes";
        }

        _demiplane.DeleteAllDemiplanes(safe: true);
        _chatSystem.DispatchGlobalAnnouncement(
            Loc.GetString(
                "cp14-round-end-monolith-discharged",
                ("time", time),
                ("units", Loc.GetString(unitsLocString))),
            announcementSound: new SoundPathSpecifier("/Audio/_CP14/Ambience/event_boom.ogg"));
    }

    private void EndRoundEndTimer()
    {
        _roundEndMoment = TimeSpan.Zero;

        _chatSystem.DispatchGlobalAnnouncement(Loc.GetString("cp14-round-end-monolith-recharged"),
            announcementSound: new SoundPathSpecifier("/Audio/_CP14/Ambience/event_boom.ogg"));
    }

    private void EndRound()
    {
        if (_gameTicker.RunLevel != GameRunLevel.InRound)
            return;

        _chatSystem.DispatchGlobalAnnouncement(Loc.GetString("cp14-round-end"),
            announcementSound: new SoundPathSpecifier("/Audio/_CP14/Ambience/event_boom.ogg"));
        _roundEndMoment = TimeSpan.Zero;
        _gameTicker.EndRound();
    }
}
