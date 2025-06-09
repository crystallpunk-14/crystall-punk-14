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

    private readonly ProtoId<CP14ReligionPrototype>? _religion = null;

    private readonly ShaderInstance _religionShader;
    private readonly Vector2[] _positions = new Vector2[MaxCount];
    private readonly float[] _radii = new float[MaxCount];
    private int _count = 0;
    public CP14ReligionVisionOverlay()
    {
        IoCManager.InjectDependencies(this);

        _religionShader = _proto.Index<ShaderPrototype>("CP14ReligionVision").InstanceUnique();

        _transform = _entManager.System<SharedTransformSystem>();

        if (_entManager.TryGetComponent<CP14ReligionEntityComponent>(_player.LocalEntity, out var vision))
        {
            _religion = vision.Religion;
        }
    }


    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        if (args.Viewport.Eye == null)
            return false;

        _count = 0;

        var clusters = new List<Cluster>();
        var religionQuery = _entManager.AllEntityQueryEnumerator<CP14ReligionObserverComponent, TransformComponent>();
        while (religionQuery.MoveNext(out var uid, out var rel, out var xform))
        {
            if (_religion is null)
                continue;

            var observation = rel.Observation;
            if (!observation.ContainsKey(_religion.Value))
                continue;

            if (!rel.Active || xform.MapID != args.MapId)
                continue;

            var mapPos = _transform.GetWorldPosition(uid);

            // To be clear, this needs to use "inside-viewport" pixels.
            // In other words, specifically NOT IViewportControl.WorldToScreen (which uses outer coordinates).
            var tempCoords = args.Viewport.WorldToLocal(mapPos);
            tempCoords.Y = args.Viewport.Size.Y - tempCoords.Y; // Local space to fragment space.

            // try find cluster to merge with
            bool merged = false;
            foreach (var cluster in clusters)
            {
                if ((cluster.Position - tempCoords).Length() < 150f)
                {
                    cluster.Add(tempCoords, rel.Observation[_religion.Value]);
                    merged = true;
                    break;
                }
            }

            if (!merged)
                clusters.Add(new Cluster(tempCoords, rel.Observation[_religion.Value]));

            if (clusters.Count >= MaxCount)
                break;
        }

        _count = 0;
        foreach (var cluster in clusters)
        {
            _positions[_count] = cluster.Position;
            _radii[_count] = cluster.Radius;
            _count++;
        }

        return true;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture == null || args.Viewport.Eye == null)
            return;

        _religionShader?.SetParameter("renderScale", args.Viewport.RenderScale * args.Viewport.Eye.Scale);
        _religionShader?.SetParameter("count", _count);
        _religionShader?.SetParameter("position", _positions);
        _religionShader?.SetParameter("radius", _radii);
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
            Radius = Math.Max(Radius, radius) + radius * 0.25f; // Радиус берется максимальный среди кластеров + надбавка.
            Count++;
        }
    }
}
