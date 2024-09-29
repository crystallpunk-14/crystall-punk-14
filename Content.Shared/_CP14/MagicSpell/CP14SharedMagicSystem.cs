using Content.Shared._CP14.MagicEnergy;
using Content.Shared._CP14.MagicEnergy.Components;
using Content.Shared._CP14.MagicSpell.Components;
using Content.Shared._CP14.MagicSpell.Events;
using Content.Shared._CP14.MagicSpell.Spells;
using Content.Shared.DoAfter;
using Content.Shared.Hands.Components;
using Content.Shared.Popups;
using Content.Shared.Speech.Muting;
using Robust.Shared.Network;
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

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14MagicEffectComponent, CP14BeforeCastMagicEffectEvent>(OnBeforeCastMagicEffect);

        SubscribeLocalEvent<CP14DelayedInstantActionEvent>(OnInstantAction);
        SubscribeLocalEvent<CP14DelayedEntityWorldTargetActionEvent>(OnEntityWorldTargetAction);

        SubscribeLocalEvent<CP14MagicEffectComponent, CP14DelayedInstantActionDoAfterEvent>(OnDelayedInstantActionDoAfter);
        SubscribeLocalEvent<CP14MagicEffectComponent, CP14DelayedEntityWorldTargetActionDoAfterEvent>(OnDelayedEntityWorldTargetDoAfter);

        SubscribeLocalEvent<CP14MagicEffectSomaticAspectComponent, CP14BeforeCastMagicEffectEvent>(OnSomaticAspectBeforeCast);

        SubscribeLocalEvent<CP14MagicEffectVerbalAspectComponent, CP14BeforeCastMagicEffectEvent>(OnVerbalAspectBeforeCast);
        SubscribeLocalEvent<CP14MagicEffectVerbalAspectComponent, CP14AfterCastMagicEffectEvent>(OnVerbalAspectAfterCast);

        SubscribeLocalEvent<CP14MagicEffectComponent, CP14AfterCastMagicEffectEvent>(OnAfterCastMagicEffect);
    }

    private void OnBeforeCastMagicEffect(Entity<CP14MagicEffectComponent> ent, ref CP14BeforeCastMagicEffectEvent args)
    {
        if (!TryComp<CP14MagicEnergyContainerComponent>(args.Performer, out var magicContainer))
        {
            args.Cancel();
            return;
        }

        if (!_magicEnergy.HasEnergy(args.Performer, ent.Comp.ManaCost, magicContainer, ent.Comp.Safe))
        {
            args.PushReason(Loc.GetString("cp14-magic-spell-not-enough-mana"));
            args.Cancel();
        }
        else if(!_magicEnergy.HasEnergy(args.Performer, ent.Comp.ManaCost, magicContainer, true) && _net.IsServer)
        {
            _popup.PopupEntity(Loc.GetString("cp14-magic-spell-not-enough-mana-cast-warning-"+_random.Next(5)), args.Performer, args.Performer, PopupType.SmallCaution);
        }
    }

    private void OnInstantAction(CP14DelayedInstantActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        if (args is not ICP14DelayedMagicEffect delayedEffect)
            return;

        if (!TryCastSpell(args.Action, args.Performer))
            return;

        var doAfterEventArgs = new DoAfterArgs(EntityManager, args.Performer, delayedEffect.Delay, new CP14DelayedInstantActionDoAfterEvent(), args.Action)
        {
            BreakOnMove = delayedEffect.BreakOnMove,
            BreakOnDamage = delayedEffect.BreakOnDamage,
            Hidden = delayedEffect.Hidden,
            BlockDuplicate = true,
            DistanceThreshold = 100f,
        };

        _doAfter.TryStartDoAfter(doAfterEventArgs);

        //Telegraphy effects
        if (_net.IsServer && TryComp<CP14MagicEffectComponent>(args.Action, out var magicEffect))
        {
            foreach (var effect in magicEffect.TelegraphyEffects)
            {
                effect.Effect(EntityManager, new CP14SpellEffectBaseArgs(args.Performer, args.Performer, Transform(args.Performer).Coordinates));
            }
        }
    }

    private void OnEntityWorldTargetAction(CP14DelayedEntityWorldTargetActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        if (args is not ICP14DelayedMagicEffect delayedEffect)
            return;

        if (!TryCastSpell(args.Action, args.Performer))
            return;

        var doAfter = new CP14DelayedEntityWorldTargetActionDoAfterEvent(
            EntityManager.GetNetCoordinates(args.Coords),
            EntityManager.GetNetEntity(args.Entity));

        var doAfterEventArgs = new DoAfterArgs(EntityManager, args.Performer, delayedEffect.Delay, doAfter, args.Action)
        {
            BreakOnMove = delayedEffect.BreakOnMove,
            BreakOnDamage = delayedEffect.BreakOnDamage,
            Hidden = delayedEffect.Hidden,
            BlockDuplicate = true,
            DistanceThreshold = 100f,
        };

        _doAfter.TryStartDoAfter(doAfterEventArgs);

        //Telegraphy effects
        if (_net.IsServer && TryComp<CP14MagicEffectComponent>(args.Action, out var magicEffect))
        {
            foreach (var effect in magicEffect.TelegraphyEffects)
            {
                effect.Effect(EntityManager, new CP14SpellEffectBaseArgs(args.Performer, args.Entity, args.Coords));
            }
        }
    }

    private void OnDelayedEntityWorldTargetDoAfter(Entity<CP14MagicEffectComponent> ent,
        ref CP14DelayedEntityWorldTargetActionDoAfterEvent args)
    {
        var endEv = new CP14EndCastMagicEffectEvent();
        RaiseLocalEvent(ent, ref endEv);

        if (args.Cancelled || !_net.IsServer)
            return;

        var targetPos = EntityManager.GetCoordinates(args.TargetPosition);
        EntityUid? targetEnt;
        EntityManager.TryGetEntity(args.TargetEntity, out targetEnt);

        var effectArgs = new CP14SpellEffectBaseArgs(args.User, targetEnt, targetPos);

        foreach (var effect in ent.Comp.Effects)
        {
            effect.Effect(EntityManager, effectArgs);
        }

        var ev = new CP14AfterCastMagicEffectEvent {Performer = args.User};
        RaiseLocalEvent(ent, ref ev);
    }

    private void OnDelayedInstantActionDoAfter(Entity<CP14MagicEffectComponent> ent, ref CP14DelayedInstantActionDoAfterEvent args)
    {
        var endEv = new CP14EndCastMagicEffectEvent();
        RaiseLocalEvent(ent, ref endEv);

        if (args.Cancelled || !_net.IsServer)
            return;

        foreach (var effect in ent.Comp.Effects)
        {
            effect.Effect(EntityManager, new CP14SpellEffectBaseArgs(args.User, args.User, Transform(args.User).Coordinates));
        }

        var ev = new CP14AfterCastMagicEffectEvent {Performer = args.User};
        RaiseLocalEvent(ent, ref ev);
    }

    private void OnSomaticAspectBeforeCast(Entity<CP14MagicEffectSomaticAspectComponent> ent, ref CP14BeforeCastMagicEffectEvent args)
    {
        if (TryComp<HandsComponent>(args.Performer, out var hands) || hands is not null)
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
        if (HasComp<MutedComponent>(args.Performer))
        {
            args.PushReason(Loc.GetString("cp14-magic-spell-need-verbal-component"));
            args.Cancel();
        }
        else
        {
            if (!args.Cancelled)
            {
                var ev = new CP14VerbalAspectSpeechEvent
                {
                    Performer = args.Performer,
                    Speech = ent.Comp.StartSpeech,
                };
                RaiseLocalEvent(ent, ref ev);
            }
        }
    }

    private void OnVerbalAspectAfterCast(Entity<CP14MagicEffectVerbalAspectComponent> ent, ref CP14AfterCastMagicEffectEvent args)
    {
        if (_net.IsClient)
            return;

        var ev = new CP14VerbalAspectSpeechEvent
        {
            Performer = args.Performer,
            Speech = ent.Comp.EndSpeech,
        };
        RaiseLocalEvent(ent, ref ev);
    }

    private bool TryCastSpell(EntityUid spell, EntityUid performer)
    {
        var ev = new CP14BeforeCastMagicEffectEvent
        {
            Performer = performer,
        };
        RaiseLocalEvent(spell, ref ev);
        if (ev.Reason != string.Empty && _net.IsServer)
        {
            _popup.PopupEntity(ev.Reason, performer, performer);
        }

        if (!ev.Cancelled)
        {
            var evStart = new CP14StartCastMagicEffectEvent()
            {
                Performer = performer,
            };
            RaiseLocalEvent(spell, ref evStart);
        }
        return !ev.Cancelled;
    }

    private void OnAfterCastMagicEffect(Entity<CP14MagicEffectComponent> ent, ref CP14AfterCastMagicEffectEvent args)
    {
        if (_net.IsClient)
            return;

        if (!HasComp<CP14MagicEnergyContainerComponent>(args.Performer))
            return;

        _magicEnergy.TryConsumeEnergy(args.Performer.Value, ent.Comp.ManaCost, safe: ent.Comp.Safe);
    }
}
