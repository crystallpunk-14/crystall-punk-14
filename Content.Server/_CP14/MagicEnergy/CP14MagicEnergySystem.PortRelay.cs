using System.Linq;
using Content.Server._CP14.MagicEnergy.Components;
using Content.Server.DeviceLinking.Systems;
using Content.Shared._CP14.MagicEnergy.Components;
using Content.Shared.DeviceLinking;

namespace Content.Server._CP14.MagicEnergy;

public partial class CP14MagicEnergySystem
{
    [Dependency] private readonly DeviceLinkSystem _signal = default!;

    [ValidatePrototypeId<SinkPortPrototype>]
    public const string PowerSinkPort = "CP14PowerTarget";
    [ValidatePrototypeId<SourcePortPrototype>]
    public const string PowerSourcePort = "CP14PowerSource";

    private void InitializePortRelay()
    {
        SubscribeLocalEvent<CP14MagicEnergyPortRelayComponent, MapInitEvent>(OnPortRelayInit);
    }

    private void UpdatePortRelay(float frameTime)
    {
        var query = EntityQueryEnumerator<CP14MagicEnergyPortRelayComponent, CP14MagicEnergyContainerComponent, DeviceLinkSourceComponent>();
        while (query.MoveNext(out var uid, out var relay, out var container, out var link))
        {
            if (relay.NextUpdateTime > _gameTiming.CurTime)
                continue;

            //Мы ищем все порты, которые связаны с этим источником, и если они равны "CP14Target", то мы передаем им энергию
            foreach (var (sinkUid, linkedPair) in link.LinkedPorts)
            {
                var passed = false;
                foreach (var (source, sink) in linkedPair)
                {
                    if (source == PowerSourcePort && sink == PowerSinkPort)
                    {
                        passed = true;
                    }
                }

                if (passed)
                    TransferEnergy((uid, container), sinkUid, relay.Energy, out _, out _, safe: relay.Safe);
            }

            relay.NextUpdateTime = _gameTiming.CurTime + TimeSpan.FromSeconds(relay.Delay);
        }
    }

    private void OnPortRelayInit(Entity<CP14MagicEnergyPortRelayComponent> ent, ref MapInitEvent args)
    {
        if (ent.Comp.SinkPort is not null)
            _signal.EnsureSinkPorts(ent, ent.Comp.SinkPort.Value);

        if (ent.Comp.SourcePort is not null)
            _signal.EnsureSourcePorts(ent, ent.Comp.SourcePort.Value);
    }
}
