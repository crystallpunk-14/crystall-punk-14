using System.Numerics;
using Content.Server.Station.Systems;
using Content.Shared._CP14.DemiplaneTraveling;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;

namespace Content.Server._CP14.DemiplaneTraveling;

public sealed partial class CP14SharedDemiplaneMapSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _userInterface = default!;
    [Dependency] private readonly StationSystem _station = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14StationDemiplaneMapComponent, ComponentInit>(OnMapInit);
        SubscribeLocalEvent<CP14DemiplaneMapComponent, BeforeActivatableUIOpenEvent>(OnBeforeActivatableUiOpen);
    }

    private void OnBeforeActivatableUiOpen(Entity<CP14DemiplaneMapComponent> ent, ref BeforeActivatableUIOpenEvent args)
    {
        var station = _station.GetOwningStation(ent, Transform(ent));

        if (!TryComp<CP14StationDemiplaneMapComponent>(station, out var stationMap))
            return;

        _userInterface.SetUiState(ent.Owner, CP14DemiplaneMapUiKey.Key, new CP14DemiplaneMapUiState(stationMap.Nodes));
    }

    private void OnMapInit(Entity<CP14StationDemiplaneMapComponent> ent, ref ComponentInit args)
    {
        GenerateDemiplaneMap(ent);
    }

    private void GenerateDemiplaneMap(Entity<CP14StationDemiplaneMapComponent> ent)
    {
        ent.Comp.Nodes.Clear();

        ent.Comp.Nodes.Add(new CP14DemiplaneMapNode("one", Vector2.Zero, "T1SwampGeode", ["RoyalPumpkin", "RoyalPumpkin"]));
        ent.Comp.Nodes.Add(new CP14DemiplaneMapNode("two", Vector2.One, "T1SwampGeode", ["RoyalPumpkin", "RoyalPumpkin"]));
        ent.Comp.Nodes.Add(new CP14DemiplaneMapNode("the", Vector2.Pi, "T1SwampGeode", ["RoyalPumpkin", "RoyalPumpkin"]));
    }
}
