using Content.Server.Popups;
using Content.Shared._CP14.Butchering;
using Content.Shared.Kitchen;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.DoAfter;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Content.Shared.Popups;
using Content.Shared.Storage;

namespace Content.Server._CP14.Butchering;

/// <summary>
/// Handles staged butchering logic.
/// Works alongside existing SharpSystem and KitchenSpikeSystem,
/// but overrides loot spawning with staged steps if component is present.
/// </summary>
public sealed class CP14ButcheringSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14ButcherableStagesComponent, SharpDoAfterEvent>(OnKnifeDoAfter);
        SubscribeLocalEvent<CP14ButcherableStagesComponent, SpikeDoAfterEvent>(OnSpikeDoAfter);
    }

    private void OnKnifeDoAfter(Entity<CP14ButcherableStagesComponent> entity, ref SharpDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Args.Target == null)
            return;

        DoStage(entity, args.Args.User);
        args.Handled = true;
    }

    private void OnSpikeDoAfter(Entity<CP14ButcherableStagesComponent> entity, ref SpikeDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Args.Target == null)
            return;

        DoStage(entity, args.Args.User);
        args.Handled = true;
    }

    /// <summary>
    /// Progresses to the next butchering stage.
    /// Spawns items and removes entity on final stage.
    /// </summary>
    private void DoStage(Entity<CP14ButcherableStagesComponent> entity, EntityUid user)
    {
        var comp = entity.Comp;
        if (comp.CurrentStage >= comp.Stages.Count)
            return;

        var stage = comp.Stages[comp.CurrentStage];
        var coords = Transform(entity).Coordinates;

        foreach (var entry in stage.Spawned)
        {
            var proto = EntitySpawnCollection.GetSpawns(new List<EntitySpawnEntry> { entry }, _random);
            foreach (var protoId in proto)
                Spawn(protoId, coords);
        }

        if (stage.FinalStage)
        {
            // Delete the entity after final stage
            Del(entity);
            _popup.PopupEntity("The body has been fully butchered.", entity, user, PopupType.LargeCaution);
            return;
        }

        comp.CurrentStage++;
        _popup.PopupEntity("You cut off a part of the body...", entity, user, PopupType.MediumCaution);
    }
}
