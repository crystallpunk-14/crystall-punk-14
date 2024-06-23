using Content.Server._CP14.Farming.Components;
using Content.Shared._CP14.DayCycle;
using Content.Shared._CP14.Farming;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._CP14.Farming;

public sealed partial class CP14FarmingSystem : CP14SharedFarmingSystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly CP14DayCycleSystem _dayCycle = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        InitializeTools();

        SubscribeLocalEvent<CP14PlantEnergyFromLightComponent, EntityUnpausedEvent>(OnUnpaused);

        SubscribeLocalEvent<CP14PlantEnergyFromLightComponent, MapInitEvent>(OnRegenerationMapInit);
    }

    private void OnUnpaused(Entity<CP14PlantEnergyFromLightComponent> ent, ref EntityUnpausedEvent args)
    {
        ent.Comp.NextUpdateTime += args.PausedTime;
    }

    private void OnRegenerationMapInit(Entity<CP14PlantEnergyFromLightComponent> regeneration, ref MapInitEvent args)
    {
        regeneration.Comp.NextUpdateTime = _timing.CurTime + TimeSpan.FromSeconds(_random.NextFloat(regeneration.Comp.MinUpdateFrequency, regeneration.Comp.MaxUpdateFrequency));
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        TakeEnergyFromLight();
    }

    private void TakeEnergyFromLight()
    {
        var query = EntityQueryEnumerator<CP14PlantComponent, CP14PlantEnergyFromLightComponent>();
        while (query.MoveNext(out var uid, out var plant, out var regeneration))
        {
            if (_timing.CurTime <= regeneration.NextUpdateTime)
                continue;

            regeneration.NextUpdateTime = _timing.CurTime +
                                          TimeSpan.FromSeconds(_random.NextFloat(regeneration.MinUpdateFrequency,
                                              regeneration.MaxUpdateFrequency));

            var gainEnergy = false;
            var daylight = _dayCycle.TryDaylightThere(uid, true);

            if (regeneration.Daylight && daylight)
                gainEnergy = true;

            if (regeneration.Dark && !daylight)
                gainEnergy = true;

            if (gainEnergy)
                plant.Energy += regeneration.Energy;
        }
    }
}
