using Content.Shared._CP14.Demiplane.Components;
using Robust.Shared.Audio;

namespace Content.Server._CP14.Demiplane;

public sealed partial class CP14DemiplaneSystem
{
    private void InitDestruction()
    {
        SubscribeLocalEvent<CP14DemiplaneTimedDestructionComponent, ComponentAdd>(OnDestructionStarted);
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
