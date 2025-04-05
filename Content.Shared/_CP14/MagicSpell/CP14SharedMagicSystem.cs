using System.Linq;
using System.Text;
using Content.Shared._CP14.MagicEnergy;
using Content.Shared._CP14.MagicEnergy.Components;
using Content.Shared._CP14.MagicSpell.Components;
using Content.Shared._CP14.MagicSpell.Events;
using Content.Shared._CP14.MagicSpell.Spells;
using Content.Shared._CP14.MagicSpellStorage;
using Content.Shared.Actions;
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
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedCP14MagicEnergySystem _magicEnergy = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly SharedActionsSystem _action = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movement = default!;
    [Dependency] private readonly StaminaSystem _stamina = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

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

        SubscribeLocalEvent<CP14MagicEffectComponent, MapInitEvent>(OnMagicEffectInit);
        SubscribeLocalEvent<CP14MagicEffectComponent, ComponentShutdown>(OnMagicEffectShutdown);

        SubscribeLocalEvent<CP14MagicEffectComponent, CP14StartCastMagicEffectEvent>(OnStartCast);
        SubscribeLocalEvent<CP14MagicEffectComponent, CP14EndCastMagicEffectEvent>(OnEndCast);

        SubscribeLocalEvent<CP14MagicEffectStaminaCostComponent, CP14MagicEffectConsumeResourceEvent>(OnStaminaConsume);
    }

    private void OnStartCast(Entity<CP14MagicEffectComponent> ent, ref CP14StartCastMagicEffectEvent args)
    {
        var caster = EnsureComp<CP14MagicCasterComponent>(args.Performer);

        caster.CastedSpells.Add(ent);
    }

    private void OnEndCast(Entity<CP14MagicEffectComponent> ent, ref CP14EndCastMagicEffectEvent args)
    {
        if (TryComp<CP14MagicCasterComponent>(args.Performer, out var caster))
        {
            caster.CastedSpells.Remove(ent);
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        UpdateToggleableActions();
    }

    /// <summary>
    /// Auto generation description for spell action
    /// </summary>
    private void OnMagicEffectInit(Entity<CP14MagicEffectComponent> ent, ref MapInitEvent args)
    {
        var meta = MetaData(ent);
        var sb = new StringBuilder();

        sb.Append(meta.EntityDescription);

        if (TryComp<CP14MagicEffectManaCostComponent>(ent, out var manaCost) && manaCost.ManaCost > 0)
        {
            sb.Append($"\n\n{Loc.GetString("cp14-magic-manacost")}: [color=#5da9e8]{manaCost.ManaCost}[/color]");
        }

        if (TryComp<CP14MagicEffectStaminaCostComponent>(ent, out var staminaCost) && staminaCost.Stamina > 0)
        {
            sb.Append($"\n\n{Loc.GetString("cp14-magic-staminacost")}: [color=#3fba54]{staminaCost.Stamina}[/color]");
        }

        if (_proto.TryIndex(ent.Comp.MagicType, out var indexedMagic))
        {
            sb.Append($"\n{Loc.GetString("cp14-magic-magic-type")}: [color={indexedMagic.Color.ToHex()}]{Loc.GetString(indexedMagic.Name)}[/color]");
        }

        if (TryComp<CP14MagicEffectVerbalAspectComponent>(ent, out var verbal))
        {
            sb.Append("\n" + Loc.GetString("cp14-magic-verbal-aspect"));
        }

        if (TryComp<CP14MagicEffectSomaticAspectComponent>(ent, out var somatic))
        {
            sb.Append("\n" + Loc.GetString("cp14-magic-somatic-aspect") + " " + somatic.FreeHandRequired);
        }

        if (TryComp<CP14MagicEffectRequiredMusicToolComponent>(ent, out var music))
        {
            sb.Append("\n" + Loc.GetString("cp14-magic-music-aspect"));
        }

        _meta.SetEntityDescription(ent, sb.ToString());
    }

    private void OnMagicEffectShutdown(Entity<CP14MagicEffectComponent> ent, ref ComponentShutdown args)
    {
        if (_doAfter.IsRunning(ent.Comp.ActiveDoAfter))
            _doAfter.Cancel(ent.Comp.ActiveDoAfter);
    }

    /// <summary>
    /// Checking to see if the spell can be used at all
    /// </summary>
    private bool CanCastSpell(Entity<CP14MagicEffectComponent> ent, CP14SpellEffectBaseArgs args)
    {
        if (args.User is not { } performer)
            return true;

        var ev = new CP14CastMagicEffectAttemptEvent(performer);
        RaiseLocalEvent(ent, ref ev);

        if (ev.Reason != string.Empty)
            _popup.PopupPredicted(ev.Reason, performer, performer);

        if (ev.Cancelled)
            return false;

        if (ent.Comp.Effects.Any(effect => !effect.CanCast(EntityManager, args)))
            return false;

        return true;
    }

    private void CastTelegraphy(Entity<CP14MagicEffectComponent> ent, CP14SpellEffectBaseArgs args)
    {
        if (!_net.IsServer)
            return;

        foreach (var effect in ent.Comp.TelegraphyEffects)
        {
            effect.Effect(EntityManager, args);
        }
    }

    private void CastSpell(Entity<CP14MagicEffectComponent> ent, CP14SpellEffectBaseArgs args)
    {
        var ev = new CP14MagicEffectConsumeResourceEvent(args.User);
        RaiseLocalEvent(ent, ref ev);

       if (_net.IsClient)
            return;

        foreach (var effect in ent.Comp.Effects)
        {
            effect.Effect(EntityManager, args);
        }
    }

    protected FixedPoint2 CalculateManacost(Entity<CP14MagicEffectManaCostComponent> ent, EntityUid? caster)
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

    private void OnStaminaConsume(Entity<CP14MagicEffectStaminaCostComponent> ent, ref CP14MagicEffectConsumeResourceEvent args)
    {
        if (args.Performer is null)
            return;

        _stamina.TakeStaminaDamage(args.Performer.Value, ent.Comp.Stamina, visual: false);
    }
}
