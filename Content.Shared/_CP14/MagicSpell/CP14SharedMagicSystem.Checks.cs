using Content.Shared._CP14.MagicSpell.Components;
using Content.Shared._CP14.MagicSpell.Events;
using Content.Shared._CP14.Religion.Components;
using Content.Shared._CP14.Religion.Prototypes;
using Content.Shared._CP14.Religion.Systems;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Damage.Components;
using Content.Shared.Hands.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Speech.Muting;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicSpell;

public abstract partial class CP14SharedMagicSystem
{
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly CP14SharedReligionGodSystem _god = default!;

    private void InitializeChecks()
    {
        SubscribeLocalEvent<CP14MagicEffectSomaticAspectComponent, CP14CastMagicEffectAttemptEvent>(OnSomaticCheck);
        SubscribeLocalEvent<CP14MagicEffectVerbalAspectComponent, CP14CastMagicEffectAttemptEvent>(OnVerbalCheck);
        SubscribeLocalEvent<CP14MagicEffectManaCostComponent, CP14CastMagicEffectAttemptEvent>(OnManaCheck);
        SubscribeLocalEvent<CP14MagicEffectStaminaCostComponent, CP14CastMagicEffectAttemptEvent>(OnStaminaCheck);
        SubscribeLocalEvent<CP14MagicEffectPacifiedBlockComponent, CP14CastMagicEffectAttemptEvent>(OnPacifiedCheck);
        SubscribeLocalEvent<CP14MagicEffectAliveTargetRequiredComponent, CP14CastMagicEffectAttemptEvent>(OnMobStateCheck);
        SubscribeLocalEvent<CP14MagicEffectReligionRestrictedComponent, CP14CastMagicEffectAttemptEvent>(OnReligionRestrictedCheck);

        //Verbal speaking
        SubscribeLocalEvent<CP14MagicEffectVerbalAspectComponent, CP14StartCastMagicEffectEvent>(OnVerbalAspectStartCast);
        SubscribeLocalEvent<CP14MagicEffectVerbalAspectComponent, CP14MagicEffectConsumeResourceEvent>(OnVerbalAspectAfterCast);
        SubscribeLocalEvent<CP14MagicEffectEmotingComponent, CP14StartCastMagicEffectEvent>(OnEmoteStartCast);
        SubscribeLocalEvent<CP14MagicEffectEmotingComponent, CP14MagicEffectConsumeResourceEvent>(OnEmoteEndCast);

    }

    /// <summary>
    /// Before using a spell, a mana check is made for the amount of mana to show warnings.
    /// </summary>
    private void OnManaCheck(Entity<CP14MagicEffectManaCostComponent> ent, ref CP14CastMagicEffectAttemptEvent args)
    {
        //Total man required
        var requiredMana = CalculateManacost(ent, args.Performer);

        //First - trying get mana from item
        if (_magicEffectQuery.TryComp(ent, out var magicEffect))
        {
            if (magicEffect.SpellStorage is not null &&
                _magicContainerQuery.TryComp(magicEffect.SpellStorage, out var magicContainer))
                requiredMana = MathF.Max(0, (float)(requiredMana - magicContainer.Energy));
        }

        if (requiredMana <= 0)
            return;

        //Second - trying get mana from performer
        if (!_magicContainerQuery.TryComp(args.Performer, out var playerMana))
        {
            args.PushReason(Loc.GetString("cp14-magic-spell-no-mana-component"));
            args.Cancel();
            return;
        }

        if (!_magicEnergy.HasEnergy(args.Performer, requiredMana, playerMana, true) && _net.IsServer)
            _popup.PopupEntity(Loc.GetString($"cp14-magic-spell-not-enough-mana-cast-warning-{_random.Next(5)}"),
                args.Performer,
                args.Performer,
                PopupType.SmallCaution);
    }

    private void OnStaminaCheck(Entity<CP14MagicEffectStaminaCostComponent> ent,
        ref CP14CastMagicEffectAttemptEvent args)
    {
        if (!TryComp<StaminaComponent>(args.Performer, out var staminaComp))
            return;

        if (!staminaComp.Critical)
            return;

        args.PushReason(Loc.GetString("cp14-magic-spell-stamina-not-enough"));
        args.Cancel();
    }

    private void OnSomaticCheck(Entity<CP14MagicEffectSomaticAspectComponent> ent,
        ref CP14CastMagicEffectAttemptEvent args)
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

    private void OnVerbalCheck(Entity<CP14MagicEffectVerbalAspectComponent> ent,
        ref CP14CastMagicEffectAttemptEvent args)
    {
        if (!HasComp<MutedComponent>(args.Performer))
            return;

        args.PushReason(Loc.GetString("cp14-magic-spell-need-verbal-component"));
        args.Cancel();
    }

    private void OnPacifiedCheck(Entity<CP14MagicEffectPacifiedBlockComponent> ent,
        ref CP14CastMagicEffectAttemptEvent args)
    {
        if (!HasComp<PacifiedComponent>(args.Performer))
            return;

        args.PushReason(Loc.GetString("cp14-magic-spell-pacified"));
        args.Cancel();
    }

    private void OnMobStateCheck(Entity<CP14MagicEffectAliveTargetRequiredComponent> ent,
        ref CP14CastMagicEffectAttemptEvent args)
    {
        if (args.Target is not { } target)
            return;

        if (!TryComp<MobStateComponent>(target, out var mobStateComp))
        {
            args.PushReason(Loc.GetString("cp14-magic-spell-target-not-mob"));
            args.Cancel();
            return;
        }

        if (!ent.Comp.Inverted)
        {
            if (_mobState.IsDead(target, mobStateComp))
            {
                args.PushReason(Loc.GetString("cp14-magic-spell-target-dead"));
                args.Cancel();
            }
        }
        else
        {
            if (!_mobState.IsDead(target, mobStateComp))
            {
                args.PushReason(Loc.GetString("cp14-magic-spell-target-alive"));
                args.Cancel();
            }
        }
    }

    private void OnReligionRestrictedCheck(Entity<CP14MagicEffectReligionRestrictedComponent> ent, ref CP14CastMagicEffectAttemptEvent args)
    {

        if (!TryComp<CP14ReligionEntityComponent>(args.Performer, out var religionComp))
            return;

        if (args.Position is not null && _god.InVision(args.Position.Value, (args.Performer, religionComp)))
            return;

        args.Cancel();
    }

    private void OnVerbalAspectStartCast(Entity<CP14MagicEffectVerbalAspectComponent> ent,
        ref CP14StartCastMagicEffectEvent args)
    {
        var ev = new CP14SpellSpeechEvent
        {
            Performer = args.Performer,
            Speech = Loc.GetString(ent.Comp.StartSpeech),
            Emote = false,
        };
        RaiseLocalEvent(ent, ref ev);
    }

    private void OnVerbalAspectAfterCast(Entity<CP14MagicEffectVerbalAspectComponent> ent,
        ref CP14MagicEffectConsumeResourceEvent args)
    {
        if (_net.IsClient)
            return;

        var ev = new CP14SpellSpeechEvent
        {
            Performer = args.Performer,
            Speech = Loc.GetString(ent.Comp.EndSpeech),
            Emote = false
        };
        RaiseLocalEvent(ent, ref ev);
    }

    private void OnEmoteStartCast(Entity<CP14MagicEffectEmotingComponent> ent, ref CP14StartCastMagicEffectEvent args)
    {
        var ev = new CP14SpellSpeechEvent
        {
            Performer = args.Performer,
            Speech = Loc.GetString(ent.Comp.StartEmote),
            Emote = true,
        };
        RaiseLocalEvent(ent, ref ev);
    }


    private void OnEmoteEndCast(Entity<CP14MagicEffectEmotingComponent> ent, ref CP14MagicEffectConsumeResourceEvent args)
    {
        if (_net.IsClient)
            return;

        var ev = new CP14SpellSpeechEvent
        {
            Performer = args.Performer,
            Speech = Loc.GetString(ent.Comp.EndEmote),
            Emote = true
        };
        RaiseLocalEvent(ent, ref ev);
    }
}
