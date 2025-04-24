using System.Linq;
using Content.Client.Gameplay;
using Content.Shared.Audio;
using Content.Shared.CCVar;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;

namespace Content.Client.Audio;

public sealed partial class ContentAudioSystem
{
    private const float AmbientLoopFadeInTime = 1f;
    private const float AmbientLoopFadeOutTime = 4f;

    private Dictionary<CP14AmbientLoopPrototype, EntityUid> _loopStreams = new();

    private TimeSpan _nextUpdateTime = TimeSpan.Zero;
    private readonly TimeSpan _updateFrequency = TimeSpan.FromSeconds(1f);

    private void CP14InitializeAmbientLoops()
    {
        Subs.CVar(_configManager, CCVars.AmbientMusicVolume, AmbienceCVarChangedAmbientMusic, true);
    }

    private void AmbienceCVarChangedAmbientMusic(float obj)
    {
        _volumeSlider = SharedAudioSystem.GainToVolume(obj);

        foreach (var loop in _loopStreams)
        {
            _audio.SetVolume(loop.Value, loop.Key.Sound.Params.Volume + _volumeSlider);
        }
    }

   private void OnRoundEndMessageAmbientLoop()
   {
       foreach (var loop in _loopStreams)
       {
           StopAmbientLoop(loop.Key);
       }
   }

   private void CP14UpdateAmbientLoops()
   {
       if (_timing.CurTime <= _nextUpdateTime)
           return;

       _nextUpdateTime = _timing.CurTime + _updateFrequency;


       if (_state.CurrentState is not GameplayState)
           return;

       var requiredLoops = GetAmbientLoops();

       foreach (var loop in _loopStreams)
       {
           if (!requiredLoops.Contains(loop.Key))  //If ambient is playing and it shouldn't, stop it.
               StopAmbientLoop(loop.Key);
       }

       foreach (var loop in requiredLoops)
       {
           if (!_loopStreams.ContainsKey(loop)) //If it's not playing, but should, run it
               StartAmbientLoop(loop);
       }
   }

   private void StartAmbientLoop(CP14AmbientLoopPrototype proto)
   {
       if (_loopStreams.ContainsKey(proto))
           return;

       var newLoop = _audio.PlayGlobal(
           proto.Sound,
           Filter.Local(),
           false,
           AudioParams.Default
               .WithLoop(true)
               .WithVolume(proto.Sound.Params.Volume + _volumeSlider)
               .WithPlayOffset(_random.NextFloat(0f, 100f)));

       if (newLoop is null)
           return;

       _loopStreams.Add(proto, newLoop.Value.Entity);

       FadeIn(newLoop.Value.Entity, newLoop.Value.Component, AmbientLoopFadeInTime);
   }

   private void StopAmbientLoop(CP14AmbientLoopPrototype proto)
   {
       if (!_loopStreams.TryGetValue(proto, out var audioEntity))
           return;

       FadeOut(audioEntity, duration: AmbientLoopFadeOutTime);
       _loopStreams.Remove(proto);
   }

   /// <summary>
   /// Checks the player's environment, and returns a list of all ambients that should currently be playing around the player
   /// </summary>
   /// <returns></returns>
   private List<CP14AmbientLoopPrototype> GetAmbientLoops()
   {
       List<CP14AmbientLoopPrototype> list = new();

       var player = _player.LocalEntity;

       if (player == null)
           return list;

       var ambientLoops = _proto.EnumeratePrototypes<CP14AmbientLoopPrototype>().ToList();

       foreach (var loop in ambientLoops)
       {
           if (_rules.IsTrue(player.Value, _proto.Index(loop.Rules)))
           {
               list.Add(loop);
           }
       }

       return list;
   }
}
