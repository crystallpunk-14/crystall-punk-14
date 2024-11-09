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
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

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

    public override void Initialize()
    {
        base.Initialize();
        InitializeActions();
        InitializeAspects();
        InitializeSlowdown();

        SubscribeLocalEvent<CP14MagicEffectComponent, MapInitEvent>(OnMagicEffectInit);
        SubscribeLocalEvent<CP14MagicEffectComponent, CP14CastMagicEffectAttemptEvent>(OnBeforeCastMagicEffect);

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
    private bool CanCastSpell(Entity<CP14MagicEffectComponent> ent, EntityUid performer)
    {
        var ev = new CP14CastMagicEffectAttemptEvent(performer);
        RaiseLocalEvent(ent, ref ev);
        if (ev.Reason != string.Empty && _net.IsServer)
        {
            _popup.PopupEntity(ev.Reason, performer, performer);
        }
        return !ev.Cancelled;
    }

    /// <summary>
    /// Before using a spell, a mana check is made for the amount of mana to show warnings.
    /// </summary>
    private void OnBeforeCastMagicEffect(Entity<CP14MagicEffectComponent> ent, ref CP14CastMagicEffectAttemptEvent args)
    {
        if (ent.Comp.SpellStorage is null) //Dont have spellStorage, we use mana from caster
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
            else if(!_magicEnergy.HasEnergy(args.Performer, manaCost, magicContainer, true) && _net.IsServer) //фу какой некрасивый хардкод
            {  //                                                                                                                    \/
                _popup.PopupEntity(Loc.GetString("cp14-magic-spell-not-enough-mana-cast-warning-"+_random.Next(5)), args.Performer, args.Performer, PopupType.SmallCaution);
            }
        }
        else //We HAVE SpellStorage, use mana from spellStorage
        {
            if (!TryComp<CP14MagicEnergyContainerComponent>(ent.Comp.SpellStorage, out var magicContainer))
            {
                args.Cancel();
                return;
            }

            var manaCost = CalculateManacost(ent, ent.Comp.SpellStorage.Value);

            if (!_magicEnergy.HasEnergy(ent.Comp.SpellStorage.Value, manaCost, magicContainer, ent.Comp.Safe))
            {
                args.PushReason(Loc.GetString("cp14-magic-spell-not-enough-mana-item"));
                args.Cancel();
            }
            else if(!_magicEnergy.HasEnergy(ent.Comp.SpellStorage.Value, manaCost, magicContainer, true) && _net.IsServer) //фу какой некрасивый хардкод
            {  //                                                                                                                  \/
                _popup.PopupEntity(Loc.GetString("cp14-magic-spell-not-enough-mana-cast-warning-item-"+_random.Next(3), ("item", MetaData(ent.Comp.SpellStorage.Value).EntityName)), args.Performer, args.Performer, PopupType.SmallCaution);
            }
        }
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

    private bool TryCastSpellDelayed(ICP14DelayedMagicEffect delayedEffect, DoAfterEvent doAfter, Entity<CP14MagicEffectComponent> action, EntityUid performer)
    {
        var doAfterEventArgs = new DoAfterArgs(EntityManager, performer, delayedEffect.CastDelay, doAfter, action, used: action.Comp.SpellStorage)
        {
            BreakOnMove = delayedEffect.BreakOnMove,
            BreakOnDamage = delayedEffect.BreakOnDamage,
            Hidden = delayedEffect.Hidden,
            DistanceThreshold = 100f,
            CancelDuplicate = true,
            BlockDuplicate = true,
            BreakOnDropItem = true,
            NeedHand = true,
        };

        return _doAfter.TryStartDoAfter(doAfterEventArgs);
    }

    private void CastSpell(Entity<CP14MagicEffectComponent> ent, CP14SpellEffectBaseArgs args, float cooldown)
    {
        _action.CP14StartCustomDelay(ent, TimeSpan.FromSeconds(cooldown));

        if (_net.IsServer)
        {
            foreach (var effect in ent.Comp.Effects)
            {
                effect.Effect(EntityManager, args);
            }
        }

        var ev = new CP14AfterCastMagicEffectEvent(args.User);
        RaiseLocalEvent(ent, ref ev);
    }

    private void OnAfterCastMagicEffect(Entity<CP14MagicEffectComponent> ent, ref CP14AfterCastMagicEffectEvent args)
    {
        if (_net.IsClient)
            return;

        if (ent.Comp.SpellStorage is null) //We pickup mana from player
        {
            if (!HasComp<CP14MagicEnergyContainerComponent>(args.Performer))
                return;

            var manaCost = CalculateManacost(ent, args.Performer.Value);
            _magicEnergy.TryConsumeEnergy(args.Performer.Value, manaCost, safe: ent.Comp.Safe);
        }
        else //We pickup mana from SpellStorage
        {
            if (!HasComp<CP14MagicEnergyContainerComponent>(ent.Comp.SpellStorage))
                return;

            var manaCost = CalculateManacost(ent, ent.Comp.SpellStorage.Value);
            _magicEnergy.TryConsumeEnergy(ent.Comp.SpellStorage.Value, manaCost, safe: ent.Comp.Safe);

            var spellEv = new CP14SpellFromSpellStorageUsedEvent(args.Performer, ent, manaCost);
            RaiseLocalEvent(ent.Comp.SpellStorage.Value, ref spellEv);
        }
    }

    private FixedPoint2 CalculateManacost(Entity<CP14MagicEffectComponent> ent, EntityUid caster)
    {
        var manaCost = ent.Comp.ManaCost;

        if (ent.Comp.CanModifyManacost)
        {
            var manaEv = new CP14CalculateManacostEvent(caster, ent.Comp.ManaCost, ent.Comp.MagicType);
            RaiseLocalEvent(caster, manaEv);

            if (ent.Comp.SpellStorage is not null)
                RaiseLocalEvent(ent.Comp.SpellStorage.Value, manaEv);

            manaCost = manaEv.GetManacost();
        }

        return manaCost;
    }
}
