using Content.Shared._CP14.WorldSprite;
using Content.Shared.Throwing;
using Robust.Client.GameObjects;
using Robust.Shared.Map.Components;

namespace Content.Client._CP14.WorldSprite;

public sealed class CP14WorldSpriteSystem : EntitySystem
{
    [Dependency] private readonly AppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

#if DEBUG
        SubscribeLocalEvent<CP14WorldSpriteComponent, ComponentInit>(OnComponentInit);
#endif

        SubscribeLocalEvent<CP14WorldSpriteComponent, EntParentChangedMessage>(OnParentChanged);
        SubscribeLocalEvent<CP14WorldSpriteComponent, ThrownEvent>(OnThrown);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CP14WorldSpriteComponent>();
        while (query.MoveNext(out var uid, out var worldSpriteComponent))
        {
            if (worldSpriteComponent.Inited)
                continue;

            Update(uid);
        }
    }

#if DEBUG
    private void OnComponentInit(Entity<CP14WorldSpriteComponent> entity, ref ComponentInit args)
    {
        if (!HasComp<AppearanceComponent>(entity))
            Log.Error($"Requires an {nameof(AppearanceComponent)} for {entity}");
    }
#endif

    private void OnParentChanged(Entity<CP14WorldSpriteComponent> entity, ref EntParentChangedMessage args)
    {
        Update(entity);
    }

    private void OnThrown(Entity<CP14WorldSpriteComponent> entity, ref ThrownEvent args)
    {
        // Idk, but throw don't call reparent
        Update(entity, args.User);
    }

    private void Update(EntityUid entity, EntityUid? parent = null)
    {
        parent ??= Transform(entity).ParentUid;

        var inWorld = HasComp<MapComponent>(parent) || HasComp<MapGridComponent>(parent);

        _appearance.SetData(entity, WorldSpriteVisualLayers.Layer, inWorld);
    }
}
