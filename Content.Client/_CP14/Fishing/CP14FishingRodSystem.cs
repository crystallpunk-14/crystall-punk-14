using Content.Client.Hands.Systems;
using Content.Shared._CP14.Fishing.Components;
using Content.Shared._CP14.Fishing.Systems;
using Robust.Client.GameObjects;
using Robust.Shared.Input;
using Robust.Shared.Timing;

namespace Content.Client._CP14.Fishing;

public sealed class CP14FishingRodSystem : CP14SharedFishingRodSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly InputSystem _input = default!;
    [Dependency] private readonly HandsSystem _hands = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        var handUid = _hands.GetActiveHandEntity();

        if (!TryComp<CP14FishingRodComponent>(handUid, out var fishingRodComponent))
            return;

        var reelKey = _input.CmdStates.GetState(EngineKeyFunctions.UseSecondary) == BoundKeyState.Down;

        if (fishingRodComponent.Reeling == reelKey)
            return;

        RaisePredictiveEvent(new RequestFishingRodReelMessage(reelKey));
    }
}
