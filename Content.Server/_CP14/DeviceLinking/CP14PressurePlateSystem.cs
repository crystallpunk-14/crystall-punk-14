using Content.Server.DeviceLinking.Systems;
using Content.Server.Storage.Components;
using Content.Shared._CP14.DeviceLinking;
using Content.Shared.Placeable;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Physics.Components;

namespace Content.Server._CP14.DeviceLinking;

public sealed class CP14PressurePlateSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly DeviceLinkSystem _signal = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<Components.CP14PressurePlateComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<Components.CP14PressurePlateComponent, ItemPlacedEvent>(OnItemPlaced);
        SubscribeLocalEvent<Components.CP14PressurePlateComponent, ItemRemovedEvent>(OnItemRemoved);
    }

    private void OnInit(Entity<Components.CP14PressurePlateComponent> plate, ref ComponentInit args)
    {
        _signal.EnsureSourcePorts(plate, plate.Comp.PressedPort, plate.Comp.ReleasedPort);
        _appearance.SetData(plate, PressurePlateVisuals.Pressed, false);
    }

    private void OnItemRemoved(Entity<Components.CP14PressurePlateComponent> plate, ref ItemRemovedEvent args)
    {
        UpdateState(plate);
    }

    private void OnItemPlaced(Entity<Components.CP14PressurePlateComponent> plate, ref ItemPlacedEvent args)
    {
        UpdateState(plate);
    }

    private void UpdateState(Entity<Components.CP14PressurePlateComponent> plate)
    {
        if (!TryComp<ItemPlacerComponent>(plate, out var itemPlacer))
            return;
        var totalMass = 0f;

        foreach (var ent in itemPlacer.PlacedEntities)
        {
            totalMass += GetEntWeightRecursive(ent);
        }
        var pressed = totalMass >= plate.Comp.WeightRequired;
        if (pressed == plate.Comp.IsPressed)
            return;

        plate.Comp.IsPressed = pressed;
        _signal.SendSignal(plate, plate.Comp.StatusPort, pressed);
        _signal.InvokePort(plate, pressed ? plate.Comp.PressedPort : plate.Comp.ReleasedPort);

        _appearance.SetData(plate, PressurePlateVisuals.Pressed, pressed);
        _audio.PlayPvs(pressed ? plate.Comp.PressedSound : plate.Comp.ReleasedSound, plate);
    }

    /// <summary>
    /// Recursively calculates the weight of the object, and all its contents, and the contents and its contents...
    /// </summary>
    public float GetEntWeightRecursive(EntityUid uid)
    {
        var totalMass = 0f;
        if (Deleted(uid)) return 0f;

        if (TryComp<PhysicsComponent>(uid, out var physics))
        {
            totalMass += physics.Mass;
        }

        //Containers
        if (TryComp<EntityStorageComponent>(uid, out var entityStorage))
        {
            var storage = entityStorage.Contents;
            foreach (var ent in storage.ContainedEntities)
            {
                totalMass += GetEntWeightRecursive(ent);
            }
        }
        //Inventory
        if (TryComp<ContainerManagerComponent>(uid, out var containerManager))
        {
            foreach (var container in containerManager.Containers)
            {
                var storage = container.Value.ContainedEntities;
                foreach (var ent in storage)
                {
                    totalMass += GetEntWeightRecursive(ent);
                }
            }
        }
        return totalMass;
    }
}
