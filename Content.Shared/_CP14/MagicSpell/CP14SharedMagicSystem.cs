using System.Text;
using Content.Shared._CP14.MagicEnergy;
using Content.Shared._CP14.MagicEnergy.Components;
using Content.Shared._CP14.MagicSpell.Components;
using Content.Shared._CP14.MagicSpell.Events;
using Content.Shared._CP14.MagicSpell.Spells;
using Content.Shared._CP14.MagicSpellStorage;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
using Content.Shared.Hands.Components;
using Content.Shared.Popups;
using Content.Shared.Speech.Muting;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared._CP14.MagicSpell;

/// <summary>
/// This system handles the basic mechanics of spell use, such as doAfter, event invocation, and energy spending.
/// </summary>
public partial class CP14SharedMagicSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedCP14MagicEnergySystem _magicEnergy = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly SharedActionsSystem _action = default!;

    public override void Initialize()
    {
        base.Initialize();
        InitializeAspects();

        SubscribeLocalEvent<CP14MagicEffectComponent, MapInitEvent>(OnMagicEffectInit);
        SubscribeLocalEvent<CP14MagicEffectComponent, CP14BeforeCastMagicEffectEvent>(OnBeforeCastMagicEffect);

        SubscribeLocalEvent<CP14DelayedInstantActionEvent>(OnInstantAction);
        SubscribeLocalEvent<CP14DelayedEntityWorldTargetActionEvent>(OnEntityWorldTargetAction);

        SubscribeLocalEvent<CP14MagicEffectComponent, CP14DelayedInstantActionDoAfterEvent>(OnDelayedInstantActionDoAfter);
        SubscribeLocalEvent<CP14MagicEffectComponent, CP14DelayedEntityWorldTargetActionDoAfterEvent>(OnDelayedEntityWorldTargetDoAfter);

        SubscribeLocalEvent<CP14MagicEffectComponent, CP14AfterCastMagicEffectEvent>(OnAfterCastMagicEffect);
    }

    /// <summary>
    /// Auto generation description for spell action
    /// </summary>
    private void OnMagicEffectInit(Entity<CP14MagicEffectComponent> ent, ref MapInitEvent args)
    {
        var meta = MetaData(ent);
        var sb = new StringBuilder();

        sb.Append(meta.EntityDescription);
        sb.Append($"\n\n {Loc.GetString("cp14-magic-manacost")}: [color=#5da9e8]{ent.Comp.ManaCost}[/color]");

        if (_proto.TryIndex(ent.Comp.MagicType, out var indexedMagic))
        {
            sb.Append($"\n {Loc.GetString("cp14-magic-magic-type")}: [color={indexedMagic.Color.ToHex()}]{Loc.GetString(indexedMagic.Name)}[/color]");
        }

        if (TryComp<CP14MagicEffectVerbalAspectComponent>(ent, out var verbal))
        {
            sb.Append("\n" + Loc.GetString("cp14-magic-verbal-aspect"));
        }

        if (TryComp<CP14MagicEffectSomaticAspectComponent>(ent, out var somatic))
        {
            sb.Append("\n" + Loc.GetString("cp14-magic-somatic-aspect") + " " + somatic.FreeHandRequired);
        }

        _meta.SetEntityDescription(ent, sb.ToString());
    }

    /// <summary>
    /// Checking to see if the spell can be used at all
    /// </summary>
    private bool CanCastSpell(EntityUid spell, EntityUid performer)
    {
        var ev = new CP14BeforeCastMagicEffectEvent(performer);
        RaiseLocalEvent(spell, ref ev);
        if (ev.Reason != string.Empty && _net.IsServer)
        {
            _popup.PopupEntity(ev.Reason, performer, performer);
        }
        return !ev.Cancelled;
    }

    /// <summary>
    /// Before using a spell, a mana check is made for the amount of mana to show warnings.
    /// </summary>
    private void OnBeforeCastMagicEffect(Entity<CP14MagicEffectComponent> ent, ref CP14BeforeCastMagicEffectEvent args)
    {
        if (!TryComp<CP14MagicEnergyContainerComponent>(args.Performer, out var magicContainer))
        {
            args.Cancel();
            return;
        }

        var manaCost = CalculateManacost(ent, args.Performer);

        if (!_magicEnergy.HasEnergy(args.Performer, manaCost, magicContainer, ent.Comp.Safe))
        {
            args.PushReason(Loc.GetString("cp14-magic-spell-not-enough-mana"));
            args.Cancel();
        }
        else if(!_magicEnergy.HasEnergy(args.Performer, manaCost, magicContainer, true) && _net.IsServer) //фу какой некрасивый  |  хардкод
        {  //                                                                                                                    \/
            _popup.PopupEntity(Loc.GetString("cp14-magic-spell-not-enough-mana-cast-warning-"+_random.Next(5)), args.Performer, args.Performer, PopupType.SmallCaution);
        }
    }

    /// <summary>
    /// Instant action used from hotkey event
    /// </summary>
    private void OnInstantAction(CP14DelayedInstantActionEvent args)
    {
        if (args.Handled)
            return;

        if (args is not ICP14DelayedMagicEffect delayedEffect)
            return;

        if (!CanCastSpell(args.Action, args.Performer))
            return;

        var spellArgs =
            new CP14SpellEffectBaseArgs(args.Performer, args.Performer, Transform(args.Performer).Coordinates);

        var evStart = new CP14StartCastMagicEffectEvent( args.Performer);
        RaiseLocalEvent(args.Action, ref evStart);

        //Telegraphy effects
        if (_net.IsServer && TryComp<CP14MagicEffectComponent>(args.Action, out var magicEffect))
        {
            foreach (var effect in magicEffect.TelegraphyEffects)
            {
                effect.Effect(EntityManager, spellArgs);
            }
        }

        if (args.CastDelay > 0)
        {
            var doAfterEventArgs = new DoAfterArgs(EntityManager, args.Performer, delayedEffect.CastDelay, new CP14DelayedInstantActionDoAfterEvent(args.Cooldown), args.Action)
            {
                BreakOnMove = delayedEffect.BreakOnMove,
                BreakOnDamage = delayedEffect.BreakOnDamage,
                Hidden = delayedEffect.Hidden,
                DistanceThreshold = 100f,
                CancelDuplicate = true,
                BlockDuplicate = true,
            };

            if (!_doAfter.TryStartDoAfter(doAfterEventArgs))
                return;
        }
        else
        {
            if (!TryComp<CP14MagicEffectComponent>(args.Action, out var magic))
                return;

            CastSpell((args.Action, magic), spellArgs, args.Cooldown);
        }

        args.Handled = true;
    }

    /// <summary>
    /// Target action used from hotkey event
    /// </summary>
    private void OnEntityWorldTargetAction(CP14DelayedEntityWorldTargetActionEvent args)
    {
        if (args.Handled)
            return;

        if (args is not ICP14DelayedMagicEffect delayedEffect)
            return;

        if (!CanCastSpell(args.Action, args.Performer))
            return;

        var doAfter = new CP14DelayedEntityWorldTargetActionDoAfterEvent(
            EntityManager.GetNetCoordinates(args.Coords),
            EntityManager.GetNetEntity(args.Entity),
            args.Cooldown);

        var spellArgs =
            new CP14SpellEffectBaseArgs(args.Performer, args.Entity, args.Coords);

        var evStart = new CP14StartCastMagicEffectEvent( args.Performer);
        RaiseLocalEvent(args.Action, ref evStart);

        //Telegraphy effects
        if (_net.IsServer && TryComp<CP14MagicEffectComponent>(args.Action, out var magicEffect))
        {
            foreach (var effect in magicEffect.TelegraphyEffects)
            {
                effect.Effect(EntityManager, spellArgs);
            }
        }

        if (args.CastDelay > 0)
        {
            var doAfterEventArgs = new DoAfterArgs(EntityManager, args.Performer, delayedEffect.CastDelay, doAfter, args.Action)
            {
                BreakOnMove = delayedEffect.BreakOnMove,
                BreakOnDamage = delayedEffect.BreakOnDamage,
                Hidden = delayedEffect.Hidden,
                DistanceThreshold = 100f,
                CancelDuplicate = true,
                BlockDuplicate = true,
            };

            if (!_doAfter.TryStartDoAfter(doAfterEventArgs))
                return;
        }
        else
        {
            if (!TryComp<CP14MagicEffectComponent>(args.Action, out var magic))
                return;

            CastSpell((args.Action, magic), spellArgs, args.Cooldown);
        }

        args.Handled = true;
    }

    //Action doAfter end
    private void OnDelayedEntityWorldTargetDoAfter(Entity<CP14MagicEffectComponent> ent,
        ref CP14DelayedEntityWorldTargetActionDoAfterEvent args)
    {
        var endEv = new CP14EndCastMagicEffectEvent(args.User);
        RaiseLocalEvent(ent, ref endEv);

        if (args.Cancelled || args.Handled)
            return;

        if (!_net.IsServer)
            return;

        var targetPos = EntityManager.GetCoordinates(args.TargetPosition);
        EntityManager.TryGetEntity(args.TargetEntity, out var targetEnt);

        CastSpell(ent, new CP14SpellEffectBaseArgs(args.User, targetEnt, targetPos), args.Cooldown ?? 0);

        args.Handled = true;
    }

    //Action doAfter end
    private void OnDelayedInstantActionDoAfter(Entity<CP14MagicEffectComponent> ent, ref CP14DelayedInstantActionDoAfterEvent args)
    {
        var endEv = new CP14EndCastMagicEffectEvent(args.User);
        RaiseLocalEvent(ent, ref endEv);

        if (args.Cancelled || args.Handled)
            return;

        if (!_net.IsServer)
            return;

        CastSpell(ent, new CP14SpellEffectBaseArgs(args.User, args.User, Transform(args.User).Coordinates), args.Cooldown ?? 0);

        args.Handled = true;
    }


    private void CastSpell(Entity<CP14MagicEffectComponent> ent, CP14SpellEffectBaseArgs args, float cooldown)
    {

        _action.CP14StartCustomDelay(ent, TimeSpan.FromSeconds(cooldown));

        if (!_net.IsServer)
            return;

        foreach (var effect in ent.Comp.Effects)
        {
            effect.Effect(EntityManager, args);
        }

        var ev = new CP14AfterCastMagicEffectEvent(args.User);
        RaiseLocalEvent(ent, ref ev);
    }



    private void OnAfterCastMagicEffect(Entity<CP14MagicEffectComponent> ent, ref CP14AfterCastMagicEffectEvent args)
    {
        if (_net.IsClient)
            return;

        if (!HasComp<CP14MagicEnergyContainerComponent>(args.Caster))
            return;

        var manaCost = CalculateManacost(ent, args.Caster.Value);
        _magicEnergy.TryConsumeEnergy(args.Caster.Value, manaCost, safe: ent.Comp.Safe);
    }

    private FixedPoint2 CalculateManacost(Entity<CP14MagicEffectComponent> ent, EntityUid caster)
    {
        var manaCost = ent.Comp.ManaCost;

        if (ent.Comp.CanModifyManacost)
        {
            var manaEv = new CP14CalculateManacostEvent(caster, ent.Comp.ManaCost, ent.Comp.MagicType);
            RaiseLocalEvent(caster, manaEv);

            if (TryComp<CP14ProvidedBySpellStorageComponent>(ent, out var provided) && provided.SpellStorage is not null)
                RaiseLocalEvent(provided.SpellStorage.Value, manaEv);

            manaCost = manaEv.GetManacost();
        }

        return manaCost;
    }
}
