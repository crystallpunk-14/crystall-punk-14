using Content.Server._CP14.WeatherControl;
using Content.Server.Weather;
using Content.Shared._CP14.Demiplane.Components;
using Content.Shared.Weather;
using Robust.Shared.Audio;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Demiplane;

public sealed partial class CP14DemiplaneSystem
{
    [Dependency] private readonly WeatherSystem _weather = default!;
    private void InitDestruction()
    {
        SubscribeLocalEvent<CP14DemiplaneTimedDestructionComponent, ComponentAdd>(OnDestructionStarted);
    }

    public void StartDestructDemiplane(Entity<CP14DemiplaneComponent> demiplane)
    {
        if (!TryComp<MapComponent>(demiplane, out var map))
            return;

        if (HasComp<CP14DemiplaneTimedDestructionComponent>(demiplane))
            return;

        EnsureComp<CP14DemiplaneTimedDestructionComponent>(demiplane);

        if (HasComp<CP14WeatherControllerComponent>(demiplane))
        {
            RemCompDeferred<CP14WeatherControllerComponent>(demiplane);
        }

        if (!_proto.TryIndex<WeatherPrototype>("CP14DemiplaneDestructionStorm", out var indexedWeather))
            return;

        _weather.SetWeather(map.MapId, indexedWeather, null);
    }

    private void OnDestructionStarted(Entity<CP14DemiplaneTimedDestructionComponent> ent, ref ComponentAdd args)
    {
        ent.Comp.EndTime = _timing.CurTime + ent.Comp.TimeToDestruction;
        ent.Comp.SelectedSong = new SoundPathSpecifier(_audio.GetSound(ent.Comp.Sound));
    }

    private void UpdateDestruction(float frameTime)
    {
        var query = EntityQueryEnumerator<CP14DemiplaneTimedDestructionComponent, CP14DemiplaneComponent>();
        while (query.MoveNext(out var uid, out var destruction, out var demiplane))
        {
            var remaining = destruction.EndTime - _timing.CurTime;

            if (destruction.SelectedSong is null)
                continue;

            var audioLength = _audio.GetAudioLength(destruction.SelectedSong.Path.ToString());

            if (destruction.Stream is null && remaining < audioLength)
            {
                var audio = _audio.PlayPvs(destruction.Sound, uid);
                destruction.Stream = audio?.Entity;
                _audio.SetMapAudio(audio);
                Dirty(uid, destruction);
                DemiplaneAnnounce(uid, Loc.GetString("cp14-demiplane-countdown", ("duration", audioLength.Minutes)));
            }

            if (remaining <= TimeSpan.Zero)
            {
                _audio.Stop(destruction.Stream);
                DeleteDemiplane((uid, demiplane));
            }
        }
    }
}
