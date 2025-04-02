using Content.Shared._CP14.Farming.Components;
using Content.Shared.Destructible;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Popups;
using Content.Shared.Tag;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Farming;

public abstract partial class CP14SharedFarmingSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedDestructibleSystem _destructible = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();
        InitializeInteractions();

        SubscribeLocalEvent<CP14PlantComponent, ExaminedEvent>(OnExamine);
    }

    private void OnExamine(EntityUid uid, CP14PlantComponent component, ExaminedEvent args)
    {
        if (component.Energy <= 0)
            args.PushMarkup(Loc.GetString("cp14-farming-low-energy"));

        if (component.Resource <= 0)
            args.PushMarkup(Loc.GetString("cp14-farming-low-resources"));
    }

    public void AffectEnergy(Entity<CP14PlantComponent> ent, float energyDelta)
    {
        if (energyDelta == 0)
            return;

        ent.Comp.Energy = MathHelper.Clamp(ent.Comp.Energy + energyDelta, 0, ent.Comp.EnergyMax);
    }
    public void AffectResource(Entity<CP14PlantComponent> ent, float resourceDelta)
    {
        if (resourceDelta == 0)
            return;

        ent.Comp.Resource = MathHelper.Clamp(ent.Comp.Resource + resourceDelta, 0, ent.Comp.ResourceMax);
    }

    public void AffectGrowth(Entity<CP14PlantComponent> ent, float growthDelta)
    {
        if (growthDelta == 0)
            return;

        ent.Comp.GrowthLevel = MathHelper.Clamp01(ent.Comp.GrowthLevel + growthDelta);
        Dirty(ent);
    }


    [Serializable, NetSerializable]
    public sealed partial class PlantSeedDoAfterEvent : SimpleDoAfterEvent
    {
    }
}
