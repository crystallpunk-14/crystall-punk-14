using Content.Shared._CP14.Religion.Components;
using Content.Shared._CP14.Religion.Prototypes;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Religion.Systems;

public abstract partial class CP14SharedReligionGodSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        base.Initialize();
        InitializeObservation();
        InitializeFollowers();
        InitializeAltars();
    }

    public HashSet<Entity<CP14ReligionEntityComponent>> GetGods(ProtoId<CP14ReligionPrototype> religion)
    {
        HashSet<Entity<CP14ReligionEntityComponent>> gods = new();

        var query = EntityQueryEnumerator<CP14ReligionEntityComponent>();
        while (query.MoveNext(out var uid, out var god))
        {
            if (god.Religion != religion)
                continue;

            gods.Add(new Entity<CP14ReligionEntityComponent>(uid, god));
        }

        return gods;
    }
}
