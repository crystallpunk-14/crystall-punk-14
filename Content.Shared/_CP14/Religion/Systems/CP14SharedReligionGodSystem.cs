using Content.Shared._CP14.Religion.Components;
using Content.Shared._CP14.Religion.Prototypes;
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

    public abstract void SendMessageToGods(ProtoId<CP14ReligionPrototype> religion, string msg, EntityUid source);
}

/// <summary>
/// It is invoked on altars and followers when they change their religion.
/// </summary>
public sealed class CP14ReligionChangedEvent(ProtoId<CP14ReligionPrototype>? oldRel, ProtoId<CP14ReligionPrototype>? newRel) : EntityEventArgs
{
    public ProtoId<CP14ReligionPrototype>? OldReligion = oldRel;
    public ProtoId<CP14ReligionPrototype>? NewReligion = newRel;
}
