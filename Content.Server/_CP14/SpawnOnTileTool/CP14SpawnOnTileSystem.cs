using Content.Shared._CP14.SpawnOnTileTool;

namespace Content.Server._CP14.SpawnOnTileTool;

public sealed partial class CP14SpawnOnTileToolSystem : SharedCP14SpawnOnTileToolSystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<CP14SpawnOnTileToolComponent, SpawnOnTileToolAfterEvent>(AfterDoAfter);
    }

    private void AfterDoAfter(Entity<CP14SpawnOnTileToolComponent> ent, ref SpawnOnTileToolAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        SpawnAtPosition(args.Spawn, GetCoordinates(args.Coordinates));

        args.Handled = true;
    }
}
