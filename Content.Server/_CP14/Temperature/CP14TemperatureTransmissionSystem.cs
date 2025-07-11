using System.Linq;
using Content.Server.Temperature.Components;
using Content.Shared.Temperature;
using Robust.Shared.Containers;

namespace Content.Server._CP14.Temperature;

public sealed class CP14TemperatureTransmissionSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14TemperatureTransmissionComponent, OnTemperatureChangeEvent>(TemperatureChangeEvent);
    }

    private void TemperatureChangeEvent(Entity<CP14TemperatureTransmissionComponent> tempTrans,
        ref OnTemperatureChangeEvent args)
    {

        if (!_container.TryGetContainer(tempTrans, tempTrans.Comp.ContainerId, out var container))
            return;

        foreach (var ent in container.ContainedEntities )
        {
            if (CompOrNull<TemperatureComponent>(ent) is { CurrentTemperature: var currentTemp } tempComp)
            {
                tempComp.CurrentTemperature = currentTemp + (args.CurrentTemperature - currentTemp) * tempTrans.Comp.TransmissionRate;
            }
        }

    }
}

