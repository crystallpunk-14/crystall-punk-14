using System.Numerics;
using Content.Shared._CP14.Religion.Components;
using Content.Shared._CP14.Religion.Prototypes;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;

namespace Content.Client._CP14.Religion;

public sealed class CP14ReligionVisionOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    private readonly SharedTransformSystem _transform;

    /// <summary>
    ///     Maximum number of observers zones that can be shown on screen at a time.
    ///     If this value is changed, the shader itself also needs to be updated.
    /// </summary>
    public const int MaxCount = 64;

    public override bool RequestScreenTexture => true;
    public override OverlaySpace Space => OverlaySpace.WorldSpace;

    private readonly ProtoId<CP14ReligionPrototype>? _religion;

    private readonly ShaderInstance _religionShader;
    private readonly Vector2[] _positions = new Vector2[MaxCount];
    private readonly float[] _radii = new float[MaxCount];
    private int _count;
    private readonly Vector2[] _antiPositions = new Vector2[MaxCount];
    private readonly float[] _antiRadii = new float[MaxCount];
    private int _antiCount;

    public CP14ReligionVisionOverlay()
    {
        IoCManager.InjectDependencies(this);

        _religionShader = _proto.Index<ShaderPrototype>("CP14ReligionVision").InstanceUnique();

        _transform = _entManager.System<SharedTransformSystem>();

        if (_entManager.TryGetComponent<CP14ReligionEntityComponent>(_player.LocalEntity, out var vision))
            _religion = vision.Religion;
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        if (args.Viewport.Eye == null)
            return false;

        var clusters = new List<Cluster>();
        var antiClusters = new List<Cluster>();
        var religionQuery = _entManager.AllEntityQueryEnumerator<CP14ReligionObserverComponent, TransformComponent>();
        while (religionQuery.MoveNext(out var uid, out var observer, out var xform))
        {
            if (_religion is null)
                continue;

            if (observer.Religion is null)
                continue;

            if (observer.Radius <= 0f)
                continue;

            if (!observer.Active || xform.MapID != args.MapId)
                continue;

            var mapPos = _transform.GetWorldPosition(uid);

            // To be clear, this needs to use "inside-viewport" pixels.
            // In other words, specifically NOT IViewportControl.WorldToScreen (which uses outer coordinates).
            var tempCoords = args.Viewport.WorldToLocal(mapPos);
            tempCoords.Y = args.Viewport.Size.Y - tempCoords.Y; // Local space to fragment space.

            if (observer.Religion.Value == _religion.Value)
            {
                // try find cluster to merge with
                bool merged = false;
                foreach (var cluster in clusters)
                {
                    if ((cluster.Position - tempCoords).Length() < 150f)
                    {
                        cluster.Add(tempCoords, observer.Radius);
                        merged = true;
                        break;
                    }
                }

                if (!merged && clusters.Count < MaxCount)
                    clusters.Add(new Cluster(tempCoords, observer.Radius));
            }
            else
            {
                // try find cluster to merge with
                bool merged = false;
                foreach (var antiCluster in antiClusters)
                {
                    if ((antiCluster.Position - tempCoords).Length() < 150f)
                    {
                        antiCluster.Add(tempCoords, observer.Radius);
                        merged = true;
                        break;
                    }
                }

                if (!merged && antiClusters.Count < MaxCount)
                    antiClusters.Add(new Cluster(tempCoords, observer.Radius));
            }
        }

        _count = 0;
        foreach (var cluster in clusters)
        {
            _positions[_count] = cluster.Position;
            _radii[_count] = cluster.Radius;
            _count++;
        }

        _antiCount = 0;
        foreach (var antiCluster in antiClusters)
        {
            _antiPositions[_antiCount] = antiCluster.Position;
            _antiRadii[_antiCount] = antiCluster.Radius;
            _antiCount++;
        }

        return true;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture == null || args.Viewport.Eye == null)
            return;

        if (!_entManager.TryGetComponent<CP14ReligionVisionComponent>(_player.LocalEntity, out var visionComponent))
            return;

        _religionShader?.SetParameter("shaderColor", visionComponent.ShaderColor);
        _religionShader?.SetParameter("renderScale", args.Viewport.RenderScale * args.Viewport.Eye.Scale);

        _religionShader?.SetParameter("count", _count);
        _religionShader?.SetParameter("position", _positions);
        _religionShader?.SetParameter("radius", _radii);

        _religionShader?.SetParameter("anticount", _antiCount);
        _religionShader?.SetParameter("antiposition", _antiPositions);
        _religionShader?.SetParameter("antiradius", _antiRadii);

        _religionShader?.SetParameter("SCREEN_TEXTURE", ScreenTexture);

        var worldHandle = args.WorldHandle;
        worldHandle.UseShader(_religionShader);
        worldHandle.DrawRect(args.WorldAABB, Color.White);
        worldHandle.UseShader(null);
    }

    private sealed class Cluster
    {
        public Vector2 Position;
        public float Radius;
        public int Count;

        public Cluster(Vector2 pos, float radius)
        {
            Position = pos;
            Radius = radius;
            Count = 1;
        }

        public void Add(Vector2 pos, float radius)
        {
            Position = (Position * Count + pos) / (Count + 1);
            Radius = Math.Max(Radius, radius);
            Count++;
        }
    }
}
