using Content.Shared._CP14.RoundStatistic;
using Content.Shared.GameTicking;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.RoundStatistic;

public sealed partial class CP14RoundStatTrackerSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;

    private Dictionary<ProtoId<CP14RoundStatTrackerPrototype>, int> _tracking = new();

    public override void Initialize()
    {
        base.Initialize();
        InitializeDemiplaneDeath();

        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundReset);
        ClearStatistic();
    }

    private void OnRoundReset(RoundRestartCleanupEvent ev)
    {
        ClearStatistic();
    }

    private void ClearStatistic()
    {
        _tracking.Clear();

        foreach (var statTracker in _proto.EnumeratePrototypes<CP14RoundStatTrackerPrototype>())
        {
            _tracking.Add(statTracker.ID, 0);
        }
    }

    public void TrackAdd(ProtoId<CP14RoundStatTrackerPrototype> proto, int dif)
    {
        _tracking[proto] += Math.Max(dif, 0);
    }

    public int GetTrack(ProtoId<CP14RoundStatTrackerPrototype> proto)
    {
        if (!_tracking.TryGetValue(proto, out var stat))
        {
            Log.Error($"Failed to get round statistic: {proto}");
            return 0;
        }

        return stat;
    }
}
