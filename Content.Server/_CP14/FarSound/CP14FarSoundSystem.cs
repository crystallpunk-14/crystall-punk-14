using Content.Server.Explosion.EntitySystems;
using Content.Shared._CP14.FarSound;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;

namespace Content.Server._CP14.FarSound;

public sealed class CP14FarSoundSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14FarSoundComponent, TriggerEvent>(OnTrigger);
    }

    private void OnTrigger(Entity<CP14FarSoundComponent> ent, ref TriggerEvent args)
    {
        var mapPos =  _transform.GetMapCoordinates(ent);
        var entPos = Transform(ent).Coordinates;
        //Play close  sound
        _audio.PlayPvs(ent.Comp.CloseSound, entPos);

        //Play far sound
        var farFilter = Filter.Empty().AddInRange(mapPos, ent.Comp.FarRange);

        _audio.PlayGlobal(ent.Comp.FarSound, farFilter, true);
    }
}
