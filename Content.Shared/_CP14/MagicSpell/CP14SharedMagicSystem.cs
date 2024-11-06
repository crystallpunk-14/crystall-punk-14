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

        SubscribeLocalEvent<CP14MagicEffectComponent, MapInitEvent>(OnMagicEffectInit);
        SubscribeLocalEvent<CP14MagicEffectComponent, CP14BeforeCastMagicEffectEvent>(OnBeforeCastMagicEffect);

        SubscribeLocalEvent<CP14DelayedInstantActionEvent>(OnInstantAction);
        SubscribeLocalEvent<CP14DelayedEntityWorldTargetActionEvent>(OnEntityWorldTargetAction);

        SubscribeLocalEvent<CP14MagicEffectComponent, CP14DelayedInstantActionDoAfterEvent>(OnDelayedInstantActionDoAfter);
        SubscribeLocalEvent<CP14MagicEffectComponent, CP14DelayedEntityWorldTargetActionDoAfterEvent>(OnDelayedEntityWorldTargetDoAfter);

        SubscribeLocalEvent<CP14MagicEffectSomaticAspectComponent, CP14BeforeCastMagicEffectEvent>(OnSomaticAspectBeforeCast);

        SubscribeLocalEvent<CP14MagicEffectVerbalAspectComponent, CP14BeforeCastMagicEffectEvent>(OnVerbalAspectBeforeCast);
        SubscribeLocalEvent<CP14MagicEffectVerbalAspectComponent, CP14StartCastMagicEffectEvent>(OnVerbalAspectStartCast);
        SubscribeLocalEvent<CP14MagicEffectVerbalAspectComponent, CP14AfterCastMagicEffectEvent>(OnVerbalAspectAfterCast);

        SubscribeLocalEvent<CP14MagicEffectComponent, CP14AfterCastMagicEffectEvent>(OnAfterCastMagicEffect);
    }

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

    private void OnBeforeCastMagicEffect(Entity<CP14MagicEffectComponent> ent, ref CP14BeforeCastMagicEffectEvent args)
    {
        if (!TryComp<CP14MagicEnergyContainerComponent>(args.Caster, out var magicContainer))
        {
            args.Cancel();
            return;
        }

        var manaCost = CalculateManacost(ent, args.Caster);

        if (!_magicEnergy.HasEnergy(args.Caster, manaCost, magicContainer, ent.Comp.Safe))
        {
            args.PushReason(Loc.GetString("cp14-magic-spell-not-enough-mana"));
            args.Cancel();
        }
        else if(!_magicEnergy.HasEnergy(args.Caster, manaCost, magicContainer, true) && _net.IsServer)
        {
            _popup.PopupEntity(Loc.GetString("cp14-magic-spell-not-enough-mana-cast-warning-"+_random.Next(5)), args.Caster, args.Caster, PopupType.SmallCaution);
        }
    }

    //Action calls
    private void OnInstantAction(CP14DelayedInstantActionEvent args)
    {
        if (args.Handled)
            return;

        if (args is not ICP14DelayedMagicEffect delayedEffect)
            return;

        if (!TryCastSpell(args.Action, args.Performer))
            return;

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

        var evStart = new CP14StartCastMagicEffectEvent( args.Performer);
        RaiseLocalEvent(args.Action, ref evStart);

        //Telegraphy effects
        if (_net.IsServer && TryComp<CP14MagicEffectComponent>(args.Action, out var magicEffect))
        {
            foreach (var effect in magicEffect.TelegraphyEffects)
            {
                effect.Effect(EntityManager, new CP14SpellEffectBaseArgs(args.Performer, args.Performer, Transform(args.Performer).Coordinates));
            }
        }

        args.Handled = true;
    }

    //Action calls
    private void OnEntityWorldTargetAction(CP14DelayedEntityWorldTargetActionEvent args)
    {
        if (args.Handled)
            return;

        if (args is not ICP14DelayedMagicEffect delayedEffect)
            return;

        if (!TryCastSpell(args.Action, args.Performer))
            return;

        var doAfter = new CP14DelayedEntityWorldTargetActionDoAfterEvent(
            EntityManager.GetNetCoordinates(args.Coords),
            EntityManager.GetNetEntity(args.Entity),
            args.Cooldown);

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

        var evStart = new CP14StartCastMagicEffectEvent( args.Performer);
        RaiseLocalEvent(args.Action, ref evStart);

        //Telegraphy effects
        if (_net.IsServer && TryComp<CP14MagicEffectComponent>(args.Action, out var magicEffect))
        {
            foreach (var effect in magicEffect.TelegraphyEffects)
            {
                effect.Effect(EntityManager, new CP14SpellEffectBaseArgs(args.Performer, args.Entity, args.Coords));
            }
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
        EntityUid? targetEnt;
        EntityManager.TryGetEntity(args.TargetEntity, out targetEnt);

        var effectArgs = new CP14SpellEffectBaseArgs(args.User, targetEnt, targetPos);

        foreach (var effect in ent.Comp.Effects)
        {
            effect.Effect(EntityManager, effectArgs);
        }

        if (args.Cooldown is not null)
            _action.CP14StartCustomDelay(ent, TimeSpan.FromSeconds(args.Cooldown.Value));

        var ev = new CP14AfterCastMagicEffectEvent(args.User);
        RaiseLocalEvent(ent, ref ev);

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

        foreach (var effect in ent.Comp.Effects)
        {
            effect.Effect(EntityManager, new CP14SpellEffectBaseArgs(args.User, args.User, Transform(args.User).Coordinates));
        }

        if (args.Cooldown is not null)
            _action.CP14StartCustomDelay(ent, TimeSpan.FromSeconds(args.Cooldown.Value));

        var ev = new CP14AfterCastMagicEffectEvent(args.User);
        RaiseLocalEvent(ent, ref ev);

        args.Handled = true;
    }






    private void OnSomaticAspectBeforeCast(Entity<CP14MagicEffectSomaticAspectComponent> ent, ref CP14BeforeCastMagicEffectEvent args)
    {
        if (TryComp<HandsComponent>(args.Caster, out var hands) || hands is not null)
        {
            var freeHand = 0;
            foreach (var hand in hands.Hands)
            {
                if (hand.Value.IsEmpty)
                    freeHand++;
            }
            if (freeHand >= ent.Comp.FreeHandRequired)
                return;
        }
        args.PushReason(Loc.GetString("cp14-magic-spell-need-somatic-component"));
        args.Cancel();
    }

    private void OnVerbalAspectBeforeCast(Entity<CP14MagicEffectVerbalAspectComponent> ent, ref CP14BeforeCastMagicEffectEvent args)
    {
        if (HasComp<MutedComponent>(args.Caster))
        {
            args.PushReason(Loc.GetString("cp14-magic-spell-need-verbal-component"));
            args.Cancel();
        }
    }

    private void OnVerbalAspectStartCast(Entity<CP14MagicEffectVerbalAspectComponent> ent, ref CP14StartCastMagicEffectEvent args)
    {
        var ev = new CP14VerbalAspectSpeechEvent
        {
            Performer = args.Caster,
            Speech = ent.Comp.StartSpeech,
        };
        RaiseLocalEvent(ent, ref ev);
    }

    private void OnVerbalAspectAfterCast(Entity<CP14MagicEffectVerbalAspectComponent> ent, ref CP14AfterCastMagicEffectEvent args)
    {
        if (_net.IsClient)
            return;

        var ev = new CP14VerbalAspectSpeechEvent
        {
            Performer = args.Caster,
            Speech = ent.Comp.EndSpeech,
        };
        RaiseLocalEvent(ent, ref ev);
    }

    private bool TryCastSpell(EntityUid spell, EntityUid performer)
    {
        var ev = new CP14BeforeCastMagicEffectEvent(performer);
        RaiseLocalEvent(spell, ref ev);
        if (ev.Reason != string.Empty && _net.IsServer)
        {
            _popup.PopupEntity(ev.Reason, performer, performer);
        }
        return !ev.Cancelled;
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
