using System.Linq;
using Content.Shared._CP14.UniqueLoot;
using Content.Shared.GameTicking;
using Content.Shared.Tag;
using Robust.Client.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._CP14.UniqueLoot;

public sealed partial class CP14UniqueLootSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private readonly Dictionary<CP14UniqueLootPrototype, int> _uniqueLootCount = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnCleanup);

        SubscribeLocalEvent<CP14UniqueLootSpawnerComponent, MapInitEvent>(OnMapInit);

        RefreshUniqueLoot();
    }

    private void OnMapInit(Entity<CP14UniqueLootSpawnerComponent> ent, ref MapInitEvent args)
    {
        var loot = GetNextUniqueLoot(ent.Comp.Tag);

        if (loot == null)
            return;

        if (TerminatingOrDeleted(ent) || !Exists(ent))
            return;

        var coords = Transform(ent).Coordinates;

        var spawned = Spawn(loot, coords);
        _transform.SetWorldRotation(spawned, _transform.GetWorldRotation(ent));
    }


    private void OnCleanup(RoundRestartCleanupEvent ev)
    {
        RefreshUniqueLoot();
    }

    private void RefreshUniqueLoot()
    {
        _uniqueLootCount.Clear();

        foreach (var loot in _proto.EnumeratePrototypes<CP14UniqueLootPrototype>())
        {
            _uniqueLootCount[loot] = loot.Count;
        }
    }

    public EntProtoId? GetNextUniqueLoot(ProtoId<TagPrototype>? withTag = null)
    {
        if (_uniqueLootCount.Count == 0)
            return null;

        var possibleLoot = _uniqueLootCount.Keys.ToList();

        CP14UniqueLootPrototype? selectedLoot = null;

        while (selectedLoot is null)
        {
            if (possibleLoot.Count == 0)
                return null;

            var tryLoot = _random.Pick(possibleLoot);

            if (withTag != null && !tryLoot.Tags.Contains(withTag.Value))
            {
                possibleLoot.Remove(tryLoot);
                continue;
            }

            selectedLoot = tryLoot;
            break;
        }



        if (_uniqueLootCount[selectedLoot] > 1)
            _uniqueLootCount[selectedLoot] -= 1;
        else
            _uniqueLootCount.Remove(selectedLoot);

        return selectedLoot.Entity;
    }
}
