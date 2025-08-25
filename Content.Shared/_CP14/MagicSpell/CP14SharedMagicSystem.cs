using System.Text;
using Content.Shared._CP14.Action.Components;
using Content.Shared._CP14.MagicEnergy;
using Content.Shared._CP14.MagicEnergy.Components;
using Content.Shared._CP14.MagicSpell.Components;
using Content.Shared._CP14.MagicSpell.Events;
using Content.Shared._CP14.MagicSpell.Spells;
using Content.Shared._CP14.MagicVision;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared._CP14.MagicSpell;

/// <summary>
/// This system handles the basic mechanics of spell use, such as doAfter, event invocation, and energy spending.
/// </summary>
public abstract partial class CP14SharedMagicSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly CP14SharedMagicEnergySystem _magicEnergy = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly SharedActionsSystem _action = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movement = default!;
    [Dependency] private readonly SharedStaminaSystem _stamina = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly CP14SharedMagicVisionSystem _magicVision = default!;
    [Dependency] private readonly INetManager _net = default!;

    private EntityQuery<CP14MagicEnergyContainerComponent> _magicContainerQuery;
    private EntityQuery<CP14MagicEffectComponent> _magicEffectQuery;

    public override void Initialize()
    {
        base.Initialize();
        InitializeDelayedActions();
        InitializeToggleableActions();
        InitializeInstantActions();
        InitializeChecks();
        InitializeSlowdown();

        _magicContainerQuery = GetEntityQuery<CP14MagicEnergyContainerComponent>();
        _magicEffectQuery = GetEntityQuery<CP14MagicEffectComponent>();

        SubscribeLocalEvent<CP14MagicEffectComponent, ComponentShutdown>(OnMagicEffectShutdown);

        SubscribeLocalEvent<CP14MagicEffectComponent, CP14StartCastMagicEffectEvent>(OnStartCast);
        SubscribeLocalEvent<CP14MagicEffectComponent, CP14EndCastMagicEffectEvent>(OnEndCast);
    }

    private void OnStartCast(Entity<CP14MagicEffectComponent> ent, ref CP14StartCastMagicEffectEvent args)
    {
        var caster = EnsureComp<CP14MagicCasterComponent>(args.Performer);

        caster.CastedSpells.Add(ent);
    }

    private void OnEndCast(Entity<CP14MagicEffectComponent> ent, ref CP14EndCastMagicEffectEvent args)
    {
        if (!_net.IsServer)
            return;

        if (!TryComp<CP14MagicCasterComponent>(args.Performer, out var caster))
            return;

        caster.CastedSpells.Remove(ent);

        //Break all casts
        List<EntityUid> castedSpells = new();
        foreach (var casted in caster.CastedSpells)
        {
            castedSpells.Add(casted);
        }

        foreach (var casted in castedSpells)
        {
            if (!_magicEffectQuery.TryComp(casted, out var castedComp))
                continue;

            _doAfter.Cancel(castedComp.ActiveDoAfter);
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        UpdateToggleableActions();
    }

    private void OnMagicEffectShutdown(Entity<CP14MagicEffectComponent> ent, ref ComponentShutdown args)
    {
        if (_doAfter.IsRunning(ent.Comp.ActiveDoAfter))
            _doAfter.Cancel(ent.Comp.ActiveDoAfter);
    }

    private void CastTelegraphy(Entity<CP14MagicEffectComponent> ent, CP14SpellEffectBaseArgs args)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        foreach (var effect in ent.Comp.TelegraphyEffects)
        {
            effect.Effect(EntityManager, args);
        }
    }

    private void CastSpell(Entity<CP14MagicEffectComponent> ent, CP14SpellEffectBaseArgs args)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        var ev = new CP14MagicEffectConsumeResourceEvent(args.User);
        RaiseLocalEvent(ent, ref ev);

        foreach (var effect in ent.Comp.Effects)
        {
            effect.Effect(EntityManager, args);
        }

        if (args.User is not null
            && TryComp<ActionComponent>(ent, out var actionComp)
            && TryComp<CP14ActionManaCostComponent>(ent, out var manaCost))
        {
            _magicVision.SpawnMagicTrace(
                Transform(args.User.Value).Coordinates,
                actionComp.Icon,
                Loc.GetString("cp14-magic-vision-used-spell", ("name", MetaData(ent).EntityName)),
                TimeSpan.FromSeconds((float)manaCost.ManaCost * 50),
                args.User,
                args.Position);
        }
    }

    public FixedPoint2 CalculateManacost(Entity<CP14ActionManaCostComponent> ent, EntityUid? caster)
    {
        var manaCost = ent.Comp.ManaCost;

        if (ent.Comp.CanModifyManacost && _magicEffectQuery.TryComp(ent, out var magicEffect))
        {
            var manaEv = new CP14CalculateManacostEvent(caster, ent.Comp.ManaCost, magicEffect.MagicType);

            if (caster is not null)
                RaiseLocalEvent(caster.Value, manaEv);

            if (magicEffect.SpellStorage is not null)
                RaiseLocalEvent(magicEffect.SpellStorage.Value, manaEv);

            manaCost = manaEv.GetManacost();
        }

        return manaCost;
    }
}
