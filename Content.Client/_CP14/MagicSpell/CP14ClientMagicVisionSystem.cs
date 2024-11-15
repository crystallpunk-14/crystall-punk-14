using Content.Shared._CP14.MagicSpell;
using Content.Shared._CP14.MagicSpell.Components;
using Robust.Client.GameObjects;
using Robust.Client.Timing;
using Robust.Shared.Console;

namespace Content.Client._CP14.MagicSpell;

public sealed class CP14ClientMagicVisionSystem : CP14SharedMagicVisionSystem
{
    [Dependency] private readonly IClientGameTiming _timing = default!;
    public bool MagicVisible { get; set; }

    private TimeSpan _nextUpdate = TimeSpan.Zero;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14MagicVisionMarkerComponent, ComponentStartup>(OnStartup);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_timing.CurTime < _nextUpdate)
            return;

        _nextUpdate = _timing.CurTime + TimeSpan.FromSeconds(1f);

        var query = EntityQueryEnumerator<CP14MagicVisionMarkerComponent, SpriteComponent>();
        while (query.MoveNext(out var uid, out var marker, out var sprite))
        {
            UpdateVisibility((uid, marker), sprite);
        }
    }

    private void OnStartup(Entity<CP14MagicVisionMarkerComponent> ent, ref ComponentStartup args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        ent.Comp.SpawnTime = _timing.CurTime;
        ent.Comp.EndTime = _timing.CurTime + ent.Comp.VisibilityTime;

        UpdateVisibility(ent, sprite);
    }

    private void UpdateVisibility(Entity<CP14MagicVisionMarkerComponent> ent, SpriteComponent sprite)
    {
        sprite.Visible = MagicVisible;



        if (MagicVisible == false)
            return;

        var progress = Math.Clamp((_timing.CurTime.TotalSeconds - ent.Comp.SpawnTime.TotalSeconds) / (ent.Comp.EndTime.TotalSeconds - ent.Comp.SpawnTime.TotalSeconds), 0, 1);
        var alpha = 1 - progress;
        Log.Info($"{ent.Owner.Id} - {alpha.ToString()}");
        sprite.Color = Color.White.WithAlpha((float)alpha);
    }
}

internal sealed class ShowMagicCommand : LocalizedCommands
{
    [Dependency] private readonly IEntitySystemManager _entitySystemManager = default!;

    public override string Command => "cp14_showmagic";

    public override string Help => "Toggle visibility of magic traces";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        _entitySystemManager.GetEntitySystem<CP14ClientMagicVisionSystem>().MagicVisible ^= true;
    }
}
