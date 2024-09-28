using System.Numerics;
using Content.Server.Decals;
using Content.Shared.Fluids;
using Content.Shared.Fluids.Components;
using Content.Shared.Maps;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.FootStep;

public sealed class CP14FootStepSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly DecalSystem _decal = default!;
    [Dependency] private readonly SharedMapSystem _maps = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDefManager = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14FootStepComponent, MoveEvent>(OnFootStepMove);
        SubscribeLocalEvent<CP14FootStepComponent, StartCollideEvent>(OnFootStepCollide);
    }

    private void OnFootStepCollide(Entity<CP14FootStepComponent> ent, ref StartCollideEvent args)
    {
        if (!TryComp<PuddleComponent>(args.OtherEntity, out var puddle))
            return;

        if (puddle.Solution is null)
            return;

        var sol = puddle.Solution;

        var splittedSol = sol.Value.Comp.Solution.SplitSolutionWithout(ent.Comp.PickSolution, SharedPuddleSystem.EvaporationReagents);

        if (splittedSol.Volume > 0)
            UpdateFootprint(ent, splittedSol.GetColor(_proto));
    }

    private void UpdateFootprint(Entity<CP14FootStepComponent> ent, Color color)
    {
        ent.Comp.DecalColor = color;
        ent.Comp.Intensity = 1f;
    }

    private void OnFootStepMove(Entity<CP14FootStepComponent> ent, ref MoveEvent args)
    {
        var distance = Vector2.Distance(args.OldPosition.Position, args.NewPosition.Position);

        ent.Comp.DistanceTraveled += distance;

        if (ent.Comp.DistanceTraveled < ent.Comp.DecalDistance)
            return;

        ent.Comp.DistanceTraveled = 0f;

        var xform = Transform(ent);
        if (!TryComp<MapGridComponent>(xform.GridUid, out var mapGrid))
            return;

        var tileRef = _maps.GetTileRef(xform.GridUid.Value, mapGrid, xform.Coordinates);
        var tileDef = (ContentTileDefinition)_tileDefManager[tileRef.Tile.TypeId];

        if (tileDef.Weather && tileDef.Color is not null)
        {
            UpdateFootprint(ent, tileDef.Color.Value);
            return;
        }

        if (ent.Comp.Intensity <= 0)
            return;

        _decal.TryAddDecal(ent.Comp.DecalProto,
            xform.Coordinates.Offset(new Vector2(-0.5f, -0.5f)),
            out var decal,
            ent.Comp.DecalColor.WithAlpha(ent.Comp.Intensity),
            xform.LocalRotation,
            cleanable: true);

        ent.Comp.Intensity = MathF.Max(0, ent.Comp.Intensity - ent.Comp.DistanceIntensityCost);
    }
}
