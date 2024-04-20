using Content.Server._CP14.Temperature;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared.Audio;
using Content.Shared.Mobs;

namespace Content.Server.Audio;

public sealed class AmbientSoundSystem : SharedAmbientSoundSystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AmbientOnPoweredComponent, PowerChangedEvent>(HandlePowerChange);
        SubscribeLocalEvent<AmbientOnPoweredComponent, PowerNetBatterySupplyEvent>(HandlePowerSupply);
        SubscribeLocalEvent<CP14FlammableAmbientSoundComponent, OnFireChangedEvent>(OnFireChanged); //CrystallPunk bonfire moment
    }

    private void HandlePowerSupply(EntityUid uid, AmbientOnPoweredComponent component, ref PowerNetBatterySupplyEvent args)
    {
        SetAmbience(uid, args.Supply);
    }

    private void HandlePowerChange(EntityUid uid, AmbientOnPoweredComponent component, ref PowerChangedEvent args)
    {
        SetAmbience(uid, args.Powered);
    }

    //CrystallPunk bonfire moment
    private void OnFireChanged(Entity<CP14FlammableAmbientSoundComponent> ent, ref OnFireChangedEvent args)
    {
        SetAmbience(ent, args.OnFire);
    }
    //CrystallPunk bonfire moment end
}
