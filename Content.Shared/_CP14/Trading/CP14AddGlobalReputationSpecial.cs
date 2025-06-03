using Content.Shared._CP14.Trading.Prototypes;
using Content.Shared._CP14.Trading.Systems;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Trading;

public sealed partial class CP14AddGlobalReputationSpecial : JobSpecial
{
    [DataField]
    public float Reputation = 1f;

    public override void AfterEquip(EntityUid mob)
    {
        var entMan = IoCManager.Resolve<IEntityManager>();
        var protoMan = IoCManager.Resolve<IPrototypeManager>();
        var tradeSys = entMan.System<CP14SharedTradingPlatformSystem>();

        foreach (var faction in protoMan.EnumeratePrototypes<CP14TradingFactionPrototype>())
        {
            tradeSys.AddReputation(mob, faction, Reputation);
        }
    }
}
