using Content.Shared._CP14.Trading.Prototypes;
using Content.Shared._CP14.Trading.Systems;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Trading;

public sealed partial class CP14AddTradingReputationSpecial : JobSpecial
{
    [DataField]
    public float Reputation = 1f;

    [DataField]
    public HashSet<ProtoId<CP14TradingFactionPrototype>> Factions = new();

    public override void AfterEquip(EntityUid mob)
    {
        var entMan = IoCManager.Resolve<IEntityManager>();
        var tradeSys = entMan.System<CP14SharedTradingPlatformSystem>();

        foreach (var faction in Factions)
        {
            tradeSys.AddReputation(mob, faction, Reputation);
        }
    }
}
