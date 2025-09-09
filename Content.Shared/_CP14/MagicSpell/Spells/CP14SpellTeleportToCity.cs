using System.Numerics;
using Content.Shared._CP14.Demiplane.Components;
using Content.Shared.Teleportation.Systems;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared._CP14.MagicSpell.Spells;

public sealed partial class CP14SpellTeleportToCity : CP14SpellEffect
{
    [DataField]
    public EntProtoId PortalProto = "CP14TempPortalToCity";
    public override void Effect(EntityManager entManager, CP14SpellEffectBaseArgs args)
    {
        if (args.Position is null)
            return;

        var net = IoCManager.Resolve<INetManager>();

        if (net.IsClient)
            return;

        var random = IoCManager.Resolve<IRobustRandom>();
        var linkSys = entManager.System<LinkedEntitySystem>();
        var query = entManager.EntityQueryEnumerator<CP14DemiplaneNavigationMapComponent, TransformComponent>();

        var first = entManager.SpawnAtPosition(PortalProto, args.Position.Value);

        while (query.MoveNext(out var uid, out var map, out var xform))
        {
            var randomOffset = new Vector2(random.Next(-1, 1), random.Next(-1, 1));
            var second = entManager.SpawnAtPosition(PortalProto, xform.Coordinates.Offset(randomOffset));

            linkSys.TryLink(first, second, true);
            return;
        }
    }
}
