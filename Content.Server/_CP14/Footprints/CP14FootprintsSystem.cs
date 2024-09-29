using System.Numerics;
using Content.Server._CP14.Footprints.Components;
using Content.Server.Decals;
using Content.Shared._CP14.Decals;
using Content.Shared.DoAfter;
using Content.Shared.Fluids;
using Content.Shared.Fluids.Components;
using Content.Shared.Interaction;
using Content.Shared.Inventory.Events;
using Content.Shared.Maps;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Footprints;

public sealed class CP14FootprintsSystem : EntitySystem
{
    [Dependency] private readonly DecalSystem _decal = default!;
    [Dependency] private readonly SharedMapSystem _maps = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDefManager = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<Components.CP14FootprintTrailerComponent, MoveEvent>(OnTrailerMove);
        SubscribeLocalEvent<Components.CP14FootprintTrailerComponent, StartCollideEvent>(OnTrailerCollide);

        SubscribeLocalEvent<CP14FootprintHolderComponent, GotEquippedEvent>(OnHolderEquipped);
        SubscribeLocalEvent<CP14FootprintHolderComponent, GotUnequippedEvent>(OnHolderUnequipped);

        SubscribeLocalEvent<CP14DecalCleanerComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<CP14DecalCleanerComponent, CP14DecalCleanerDoAfterEvent>(OnCleanDoAfter);
    }

    private void OnCleanDoAfter(Entity<CP14DecalCleanerComponent> ent, ref CP14DecalCleanerDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        var gridUid = Transform(ent).GridUid;
        if (gridUid is null)
            return;

        if (!TryComp<MapGridComponent>(gridUid, out var map))
            return;

        var coord = EntityManager.GetCoordinates(args.ClickLocation);
        _audio.PlayPvs(ent.Comp.Sound, coord);
        SpawnAtPosition(ent.Comp.SpawnEffect, coord);

        var oldDecals = _decal.GetDecalsInRange(gridUid.Value, args.ClickLocation.Position, ent.Comp.Range);
        foreach (var (id, decal) in oldDecals)
        {
            if (decal.Cleanable)
                _decal.RemoveDecal(gridUid.Value, id);
        }

        args.Handled = true;
    }

    private void OnAfterInteract(Entity<CP14DecalCleanerComponent> ent, ref AfterInteractEvent args)
    {
        if (!args.CanReach && !args.Handled)
            return;

        var doAfter = new DoAfterArgs(EntityManager,
            args.User,
            ent.Comp.Delay,
            new CP14DecalCleanerDoAfterEvent(EntityManager.GetNetCoordinates(args.ClickLocation)),
            ent)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = true,
        };
        _doAfter.TryStartDoAfter(doAfter);

        args.Handled = true;
    }

    private void OnHolderUnequipped(Entity<CP14FootprintHolderComponent> ent, ref GotUnequippedEvent args)
    {
        if (!TryComp<Components.CP14FootprintTrailerComponent>(args.Equipee, out var trailer))
            return;

        trailer.holder = null;

        if (TryComp<CP14FootprintHolderComponent>(args.Equipee, out var selfHolder))
        {
            trailer.holder = selfHolder;
        }
    }

    private void OnHolderEquipped(Entity<CP14FootprintHolderComponent> ent, ref GotEquippedEvent args)
    {
        if (!TryComp<Components.CP14FootprintTrailerComponent>(args.Equipee, out var trailer))
            return;

        trailer.holder = ent.Comp;
    }

    private void OnTrailerCollide(Entity<Components.CP14FootprintTrailerComponent> ent, ref StartCollideEvent args)
    {
        if (ent.Comp.holder is null)
            return;
        var footprint = ent.Comp.holder;

        if (!TryComp<PuddleComponent>(args.OtherEntity, out var puddle))
            return;

        if (puddle.Solution is null)
            return;

        var sol = puddle.Solution;

        var splittedSol = sol.Value.Comp.Solution.SplitSolutionWithout(footprint.PickSolution, SharedPuddleSystem.EvaporationReagents);

        if (splittedSol.Volume > 0)
            UpdateFootprint(footprint, splittedSol.GetColor(_proto));
    }

    private void UpdateFootprint(CP14FootprintHolderComponent comp, Color color)
    {
        comp.DecalColor = color;
        comp.Intensity = 1f;
    }

    private void OnTrailerMove(Entity<Components.CP14FootprintTrailerComponent> ent, ref MoveEvent args)
    {
        if (ent.Comp.holder is null)
            return;
        var footprint = ent.Comp.holder;

        var distance = Vector2.Distance(args.OldPosition.Position, args.NewPosition.Position);

        footprint.DistanceTraveled += distance;

        if (footprint.DistanceTraveled < footprint.DecalDistance)
            return;

        footprint.DistanceTraveled = 0f;

        var xform = Transform(ent);
        if (!TryComp<MapGridComponent>(xform.GridUid, out var mapGrid))
            return;

        var tileRef = _maps.GetTileRef(xform.GridUid.Value, mapGrid, xform.Coordinates);
        var tileDef = (ContentTileDefinition)_tileDefManager[tileRef.Tile.TypeId];

        if (tileDef.Weather && tileDef.Color is not null)
        {
            UpdateFootprint(footprint, tileDef.Color.Value);
            return;
        }

        if (footprint.Intensity <= 0)
            return;

        _decal.TryAddDecal(footprint.DecalProto,
            xform.Coordinates.Offset(new Vector2(-0.5f, -0.5f)),
            out var decal,
            footprint.DecalColor.WithAlpha(footprint.Intensity),
            xform.LocalRotation,
            cleanable: true);

        footprint.Intensity = MathF.Max(0, footprint.Intensity - footprint.DistanceIntensityCost);
    }
}
