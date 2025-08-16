using System.Numerics;
using Content.Shared._CP14.UniqueLoot;
using Content.Shared._CP14.Vampire;
using Content.Shared.Teleportation.Systems;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared._CP14.MagicSpell.Spells;

public sealed partial class CP14SpellTeleportToVampireSingleton : CP14SpellEffect
{
    [DataField]
    public EntProtoId PortalProto = "CP14TempPortalRed";

    public override void Effect(EntityManager entManager, CP14SpellEffectBaseArgs args)
    {
        if (args.Position is null)
            return;
        if (args.User is null)
            return;

        if (!entManager.TryGetComponent<CP14VampireComponent>(args.User.Value, out var vampireComponent))
            return;

        var net = IoCManager.Resolve<INetManager>();

        if (net.IsClient)
            return;

        var random = IoCManager.Resolve<IRobustRandom>();
        var linkSys = entManager.System<LinkedEntitySystem>();
        var query = entManager.EntityQueryEnumerator<CP14SingletonComponent, TransformComponent>();

        var first = entManager.SpawnAtPosition(PortalProto, args.Position.Value);

        while (query.MoveNext(out var uid, out var singleton, out var xform))
        {
            if (singleton.Key != vampireComponent.SingletonTeleportKey)
                continue;

            var randomOffset = new Vector2(random.Next(-1, 1), random.Next(-1, 1));
            var second = entManager.SpawnAtPosition(PortalProto, xform.Coordinates.Offset(randomOffset));

            linkSys.TryLink(first, second, true);
            return;
        }
    }
}
