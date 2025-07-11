
using Content.Server.Temperature.Components;
using Content.Shared.Temperature;
using Robust.Shared.Containers;

namespace Content.Server._CP14.Temperature;

/// <summary>
/// Allows you to transfer heat from object to objects in the container.
/// </summary>
public sealed class CP14TemperatureTransmissionSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;

    //TODO: When the temperature control system is updated, it will need to be replaced with Update.
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

        if (container.ContainedEntities.Count == 0)
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

