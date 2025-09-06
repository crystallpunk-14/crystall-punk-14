using System.Text;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Server.Temperature.Components;
using Content.Server.Temperature.Systems;
using Content.Shared._CP14.DayCycle;
using Content.Shared._CP14.Vampire;
using Content.Shared._CP14.Vampire.Components;
using Content.Shared.Examine;
using Content.Shared.FixedPoint;
using Content.Shared.Ghost;
using Content.Shared.Popups;
using Content.Shared.Stacks;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Events;
using Robust.Shared.Timing;

namespace Content.Server._CP14.Vampire;

public sealed partial class CP14VampireSystem : CP14SharedVampireSystem
{
    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly TemperatureSystem _temperature = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly CP14DayCycleSystem _dayCycle = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();
        InitializeAnnounces();

        SubscribeLocalEvent<CP14VampireClanHeartComponent, StartCollideEvent>(OnStartCollide);
        SubscribeLocalEvent<CP14VampireClanHeartComponent, ExaminedEvent>(OnExamined);
    }

    private void OnStartCollide(Entity<CP14VampireClanHeartComponent> ent, ref StartCollideEvent args)
    {
        if (!TryComp<CP14VampireTreeCollectableComponent>(args.OtherEntity, out var collectable))
            return;

        var collect = collectable.Essence;

        if (TryComp<StackComponent>(args.OtherEntity, out var stack))
            collect *= stack.Count;

        AddEssence(ent, collect);
        Del(args.OtherEntity);

        _audio.PlayPvs(collectable.CollectSound, ent);
    }

    private void OnExamined(Entity<CP14VampireClanHeartComponent> ent, ref ExaminedEvent args)
    {
        if (!HasComp<CP14VampireComponent>(args.Examiner) && !HasComp<GhostComponent>(args.Examiner))
            return;

        var sb = new StringBuilder();

        // Faction
        if (Proto.TryIndex(ent.Comp.Faction, out var indexedFaction))
            sb.Append(Loc.GetString("cp14-vampire-tree-examine-faction", ("faction", Loc.GetString(indexedFaction.Name))) + "\n");

        // Are they friend or foe?
        if (TryComp<CP14VampireComponent>(args.Examiner, out var examinerVampire))
        {
            if (examinerVampire.Faction == ent.Comp.Faction)
                sb.Append(Loc.GetString("cp14-vampire-tree-examine-friend") + "\n");
            else
                sb.Append(Loc.GetString("cp14-vampire-tree-examine-enemy") + "\n");
        }

        //Progress
        sb.Append(Loc.GetString("cp14-vampire-tree-examine-level",
            ("level", ent.Comp.Level),
            ("essence", ent.Comp.EssenceFromLevelStart),
            ("left", ent.Comp.EssenceToNextLevel?.ToString() ?? "∞")) + "\n"+ "\n");

        var query = EntityQueryEnumerator<CP14VampireClanHeartComponent>();

        sb.Append(Loc.GetString("cp14-vampire-tree-other-title") + "\n");
        while (query.MoveNext(out var uid, out var heart))
        {
            if (uid == ent.Owner)
                continue;

            if (!Proto.TryIndex(heart.Faction, out var indexedOtherFaction))
                continue;

            sb.Append(Loc.GetString("cp14-vampire-tree-other-info",
                ("name", Loc.GetString(indexedOtherFaction.Name)),
                ("essence", heart.EssenceFromLevelStart),
                ("left", heart.EssenceToNextLevel?.ToString() ?? "∞"),
                ("lvl", heart.Level)) + "\n");
        }

        args.PushMarkup(sb.ToString());
    }

    private void AddEssence(Entity<CP14VampireClanHeartComponent> ent, FixedPoint2 amount)
    {
        if (!Proto.TryIndex(ent.Comp.Faction, out var indexedFaction) || ent.Comp.Faction == null)
            return;

        var level = ent.Comp.Level;

        ent.Comp.CollectedEssence += amount;
        Dirty(ent);

        if (level < ent.Comp.Level) //Level up!
        {
            _appearance.SetData(ent, VampireClanLevelVisuals.Level, ent.Comp.Level);
            AnnounceToOpposingFactions(ent.Comp.Faction.Value, Loc.GetString("cp14-vampire-tree-growing", ("name", Loc.GetString(indexedFaction.Name)), ("level", ent.Comp.Level)));
            AnnounceToFaction(ent.Comp.Faction.Value, Loc.GetString("cp14-vampire-tree-growing-self", ("level", ent.Comp.Level)));

            SpawnAtPosition(ent.Comp.LevelUpVfx, Transform(ent).Coordinates);
        }
    }

    protected override void OnVampireInit(Entity<CP14VampireComponent> ent, ref MapInitEvent args)
    {
        base.OnVampireInit(ent, ref args);

        //Metabolism
        foreach (var (organUid, _) in _body.GetBodyOrgans(ent))
        {
            if (TryComp<MetabolizerComponent>(organUid, out var metabolizer) && metabolizer.MetabolizerTypes is not null)
            {
                metabolizer.MetabolizerTypes.Add(ent.Comp.MetabolizerType);
            }
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        //Vampire under sun heating
        var query = EntityQueryEnumerator<CP14VampireComponent, CP14VampireVisualsComponent, TemperatureComponent, FlammableComponent>();
        while (query.MoveNext(out var uid, out var vampire, out var visuals, out var temperature, out var flammable))
        {
            if (_timing.CurTime < vampire.NextHeatTime)
                continue;

            vampire.NextHeatTime = _timing.CurTime + vampire.HeatFrequency;

            if (!_dayCycle.UnderSunlight(uid))
                continue;

            _temperature.ChangeHeat(uid, vampire.HeatUnderSunTemperature);
            _popup.PopupEntity(Loc.GetString("cp14-heat-under-sun"), uid, uid, PopupType.SmallCaution);

            if (temperature.CurrentTemperature > vampire.IgniteThreshold && !flammable.OnFire)
            {
                _flammable.AdjustFireStacks(uid, 1, flammable);
                _flammable.Ignite(uid, uid, flammable);
            }
        }

        var heartQuery = EntityQueryEnumerator<CP14VampireClanHeartComponent>();
        //regen essence over time
        while (heartQuery.MoveNext(out var uid, out var heart))
        {
            if (_timing.CurTime < heart.NextRegenTime)
                continue;

            heart.NextRegenTime = _timing.CurTime + heart.RegenFrequency;

            AddEssence((uid, heart), heart.EssenceRegen);
        }
    }
}
