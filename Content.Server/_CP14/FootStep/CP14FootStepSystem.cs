using System.Numerics;
using Content.Server.Decals;
using Content.Shared.Maps;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Server._CP14.FootStep;

public sealed class CP14FootStepSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly DecalSystem _decal = default!;
    [Dependency] private readonly SharedMapSystem _maps = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDefManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14FootStepComponent, MoveEvent>(OnFootStepMove);
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
            ent.Comp.DecalColor = tileDef.Color.Value;
            ent.Comp.Intensity = 1f;
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
