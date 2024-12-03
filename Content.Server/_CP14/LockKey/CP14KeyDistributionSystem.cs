using Content.Server.Station.Events;
using Content.Shared._CP14.LockKey;
using Content.Shared._CP14.LockKey.Components;
using Content.Shared.Station.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Server._CP14.LockKey;

public sealed partial class CP14KeyDistributionSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly CP14KeyholeGenerationSystem _keyGeneration = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14AbstractKeyComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<CP14AbstractKeyComponent> ent, ref MapInitEvent args)
    {
        if (!TrySetShape(ent) && ent.Comp.DeleteOnFailure)
            QueueDel(ent);
    }

    private bool TrySetShape(Entity<CP14AbstractKeyComponent> ent)
    {
        var grid = Transform(ent).GridUid;

        if (grid is null)
            return false;

        if (!TryComp<CP14KeyComponent>(ent, out var key))
            return false;

        if (!TryComp<StationMemberComponent>(grid.Value, out var member))
            return false;

        if (!TryComp<CP14StationKeyDistributionComponent>(member.Station, out var distribution))
            return false;

        var keysList = new List<ProtoId<CP14LockTypePrototype>>(distribution.Keys);
        while (keysList.Count > 0)
        {
            var randomIndex = _random.Next(keysList.Count);
            var keyA = keysList[randomIndex];

            var indexedKey = _proto.Index(keyA);

            if (indexedKey.Group != ent.Comp.Group)
            {
                keysList.RemoveAt(randomIndex);
                continue;
            }

            _keyGeneration.SetShape((ent, key), indexedKey);
            distribution.Keys.Remove(indexedKey);
            return true;
        }

        return false;
    }
}
