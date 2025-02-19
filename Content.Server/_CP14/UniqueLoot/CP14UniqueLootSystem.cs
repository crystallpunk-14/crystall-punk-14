using Content.Server.GameTicking.Events;
using Content.Shared._CP14.UniqueLoot;
using Content.Shared.GameTicking;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._CP14.UniqueLoot;

public sealed partial class CP14UniqueLootSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    private Dictionary<ProtoId<CP14UniqueLootPrototype>, int> _uniqueLootCount = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundStartingEvent>(OnRoundStart);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnCleanup);

        SubscribeLocalEvent<CP14UniqueLootSpawnerComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<CP14UniqueLootSpawnerComponent> ent, ref MapInitEvent args)
    {
        var loot = GetNextUniqueLoot();

        if (loot == null)
            return;

        if (!Deleted(ent))
            SpawnAtPosition(loot, Transform(ent).Coordinates);

        if (!TerminatingOrDeleted(ent) && Exists(ent))
            QueueDel(ent);
    }

    private void OnRoundStart(RoundStartingEvent ev)
    {
        RefreshUniqueLoot();
    }

    private void OnCleanup(RoundRestartCleanupEvent ev)
    {
        _uniqueLootCount.Clear();
    }

    private void RefreshUniqueLoot()
    {
        _uniqueLootCount.Clear();

        foreach (var loot in _proto.EnumeratePrototypes<CP14UniqueLootPrototype>())
        {
            _uniqueLootCount[loot.ID] = loot.Count;
        }
    }

    public EntProtoId? GetNextUniqueLoot()
    {
        if (_uniqueLootCount.Count == 0)
            return null;

        var selectedLoot = _random.Pick(_uniqueLootCount);

        //TODO: Tag filtering

        if (selectedLoot.Value > 1)
            _uniqueLootCount[selectedLoot.Key] -= 1;
        else
            _uniqueLootCount.Remove(selectedLoot.Key);

        if (!_proto.TryIndex(selectedLoot.Key, out var indexedLoot))
            return null;

        return indexedLoot.Entity;
    }
}
