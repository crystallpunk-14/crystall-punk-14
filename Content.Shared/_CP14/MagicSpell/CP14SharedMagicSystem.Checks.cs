using System.Linq;
using Content.Shared._CP14.MagicEnergy.Components;
using Content.Shared._CP14.MagicSpell.Components;
using Content.Shared._CP14.MagicSpell.Events;
using Content.Shared._CP14.Religion.Components;
using Content.Shared._CP14.Religion.Systems;
using Content.Shared._CP14.Skill;
using Content.Shared._CP14.Skill.Components;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Damage.Components;
using Content.Shared.FixedPoint;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Speech.Muting;

namespace Content.Shared._CP14.MagicSpell;

public abstract partial class CP14SharedMagicSystem
{
    [Dependency] private readonly CP14SharedReligionGodSystem _god = default!;
    [Dependency] private readonly SharedHandsSystem _hand = default!;
    [Dependency] private readonly CP14SharedSkillSystem _skill = default!;

    private void InitializeChecks()
    {
        SubscribeLocalEvent<CP14MagicEffectSomaticAspectComponent, CP14CastMagicEffectAttemptEvent>(OnSomaticCheck);
        SubscribeLocalEvent<CP14MagicEffectVerbalAspectComponent, CP14CastMagicEffectAttemptEvent>(OnVerbalCheck);
        SubscribeLocalEvent<CP14MagicEffectMaterialAspectComponent, CP14CastMagicEffectAttemptEvent>(OnMaterialCheck);
        SubscribeLocalEvent<CP14MagicEffectManaCostComponent, CP14CastMagicEffectAttemptEvent>(OnManaCheck);
        SubscribeLocalEvent<CP14MagicEffectStaminaCostComponent, CP14CastMagicEffectAttemptEvent>(OnStaminaCheck);
        SubscribeLocalEvent<CP14MagicEffectSkillPointCostComponent, CP14CastMagicEffectAttemptEvent>(OnSkillPointCheck);
        SubscribeLocalEvent<CP14MagicEffectPacifiedBlockComponent, CP14CastMagicEffectAttemptEvent>(OnPacifiedCheck);
        SubscribeLocalEvent<CP14MagicEffectTargetMobStatusRequiredComponent, CP14CastMagicEffectAttemptEvent>(OnMobStateCheck);
        SubscribeLocalEvent<CP14MagicEffectReligionRestrictedComponent, CP14CastMagicEffectAttemptEvent>(OnReligionRestrictedCheck);

        //Verbal speaking
        SubscribeLocalEvent<CP14MagicEffectVerbalAspectComponent, CP14StartCastMagicEffectEvent>(OnVerbalAspectStartCast);
        SubscribeLocalEvent<CP14MagicEffectVerbalAspectComponent, CP14MagicEffectConsumeResourceEvent>(OnVerbalAspectAfterCast);

        SubscribeLocalEvent<CP14MagicEffectEmotingComponent, CP14StartCastMagicEffectEvent>(OnEmoteStartCast);
        SubscribeLocalEvent<CP14MagicEffectEmotingComponent, CP14MagicEffectConsumeResourceEvent>(OnEmoteEndCast);

        //Consuming resources
        SubscribeLocalEvent<CP14MagicEffectMaterialAspectComponent, CP14MagicEffectConsumeResourceEvent>(OnMaterialAspectEndCast);
        SubscribeLocalEvent<CP14MagicEffectStaminaCostComponent, CP14MagicEffectConsumeResourceEvent>(OnStaminaConsume);
        SubscribeLocalEvent<CP14MagicEffectManaCostComponent, CP14MagicEffectConsumeResourceEvent>(OnManaConsume);
        SubscribeLocalEvent<CP14MagicEffectSkillPointCostComponent, CP14MagicEffectConsumeResourceEvent>(OnSkillPointConsume);
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

        if (!_magicEnergy.HasEnergy(args.Performer, requiredMana, playerMana, true))
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

    private void OnSkillPointCheck(Entity<CP14MagicEffectSkillPointCostComponent> ent, ref CP14CastMagicEffectAttemptEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.SkillPoint, out var indexedSkillPoint) || ent.Comp.SkillPoint is null)
            return;

        if (!TryComp<CP14SkillStorageComponent>(args.Performer, out var skillStorage))
        {
            args.PushReason(Loc.GetString("cp14-magic-spell-skillpoint-not-enough", ("name", Loc.GetString(indexedSkillPoint.Name)), ("count", ent.Comp.Count)));
            args.Cancel();
            return;
        }

        var points = skillStorage.SkillPoints;
        if (points.TryGetValue(ent.Comp.SkillPoint.Value, out var currentPoints))
        {
            var freePoints = currentPoints.Max - currentPoints.Sum;

            if (freePoints < ent.Comp.Count)
            {
                var d = ent.Comp.Count - freePoints;

                args.PushReason(Loc.GetString("cp14-magic-spell-skillpoint-not-enough",
                    ("name", Loc.GetString(indexedSkillPoint.Name)),
                    ("count", d)));
                args.Cancel();
            }
        }
    }

    private void OnSomaticCheck(Entity<CP14MagicEffectSomaticAspectComponent> ent,
        ref CP14CastMagicEffectAttemptEvent args)
    {
        if (TryComp<HandsComponent>(args.Performer, out var hands) || hands is not null)
        {
            if (_hand.CountFreeableHands((args.Performer, hands)) >= ent.Comp.FreeHandRequired)
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

    private void OnMaterialCheck(Entity<CP14MagicEffectMaterialAspectComponent> ent, ref CP14CastMagicEffectAttemptEvent args)
    {
        if (ent.Comp.Requirement is null)
            return;

        HashSet<EntityUid> heldedItems = new();

        foreach (var hand in _hand.EnumerateHands(args.Performer))
        {
            var helded = _hand.GetHeldItem(args.Performer, hand);
            if (helded is not null)
                heldedItems.Add(helded.Value);
        }

        if (!ent.Comp.Requirement.CheckRequirement(EntityManager, _proto, heldedItems))
        {
            args.PushReason(Loc.GetString("cp14-magic-spell-need-material-component"));
            args.Cancel();
        }
    }

    private void OnPacifiedCheck(Entity<CP14MagicEffectPacifiedBlockComponent> ent,
        ref CP14CastMagicEffectAttemptEvent args)
    {
        if (!HasComp<PacifiedComponent>(args.Performer))
            return;

        args.PushReason(Loc.GetString("cp14-magic-spell-pacified"));
        args.Cancel();
    }

    private void OnMobStateCheck(Entity<CP14MagicEffectTargetMobStatusRequiredComponent> ent,
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

        if (!ent.Comp.AllowedStates.Contains(mobStateComp.CurrentState))
        {
            var states = string.Join(", ",
                ent.Comp.AllowedStates.Select(state => state switch
                {
                    MobState.Alive => Loc.GetString("cp14-magic-spell-target-mob-state-live"),
                    MobState.Dead => Loc.GetString("cp14-magic-spell-target-mob-state-dead"),
                    MobState.Critical => Loc.GetString("cp14-magic-spell-target-mob-state-critical")
                }));

            args.PushReason(Loc.GetString("cp14-magic-spell-target-mob-state", ("state", states)));
            args.Cancel();
        }
    }

    private void OnReligionRestrictedCheck(Entity<CP14MagicEffectReligionRestrictedComponent> ent,
        ref CP14CastMagicEffectAttemptEvent args)
    {
        if (!TryComp<CP14ReligionEntityComponent>(args.Performer, out var religionComp))
            return;

        var position = args.Position;

        if (args.Target is not null)
            position ??= Transform(args.Target.Value).Coordinates;

        if (ent.Comp.OnlyInReligionZone)
        {
            if (position is null || !_god.InVision(position.Value, (args.Performer, religionComp)))
            {
                args.Cancel();
            }
        }

        if (ent.Comp.OnlyOnFollowers)
        {
            if (args.Target is null || !TryComp<CP14ReligionFollowerComponent>(args.Target, out var follower) || follower.Religion != religionComp.Religion)
            {
                args.PushReason(Loc.GetString("cp14-magic-spell-target-god-follower"));
                args.Cancel();
            }
        }
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
        var ev = new CP14SpellSpeechEvent
        {
            Performer = args.Performer,
            Speech = Loc.GetString(ent.Comp.EndEmote),
            Emote = true
        };
        RaiseLocalEvent(ent, ref ev);
    }

    private void OnMaterialAspectEndCast(Entity<CP14MagicEffectMaterialAspectComponent> ent, ref CP14MagicEffectConsumeResourceEvent args)
    {
        if (ent.Comp.Requirement is null || args.Performer is null)
            return;

        HashSet<EntityUid> heldedItems = new();

        foreach (var hand in _hand.EnumerateHands(args.Performer.Value))
        {
            var helded = _hand.GetHeldItem(args.Performer.Value, hand);
            if (helded is not null)
                heldedItems.Add(helded.Value);
        }

        ent.Comp.Requirement.PostCraft(EntityManager, _proto, heldedItems);
    }

    private void OnStaminaConsume(Entity<CP14MagicEffectStaminaCostComponent> ent, ref CP14MagicEffectConsumeResourceEvent args)
    {
        if (args.Performer is null)
            return;

        _stamina.TakeStaminaDamage(args.Performer.Value, ent.Comp.Stamina, visual: false);
    }

    private void OnManaConsume(Entity<CP14MagicEffectManaCostComponent> ent, ref CP14MagicEffectConsumeResourceEvent args)
    {
        if (!TryComp<CP14MagicEffectComponent>(ent, out var magicEffect))
            return;

        var requiredMana = CalculateManacost(ent, args.Performer);

        //First - used object
        if (magicEffect.SpellStorage is not null && TryComp<CP14MagicEnergyContainerComponent>(magicEffect.SpellStorage, out var magicStorage))
        {
            var spellEv = new CP14SpellFromSpellStorageUsedEvent(args.Performer, (ent, magicEffect), requiredMana);
            RaiseLocalEvent(magicEffect.SpellStorage.Value, ref spellEv);

            _magicEnergy.ChangeEnergy((magicEffect.SpellStorage.Value, magicStorage), -requiredMana, out var changedEnergy, out var overloadedEnergy, safe: false);
            requiredMana -= FixedPoint2.Abs(changedEnergy + overloadedEnergy);
        }

        //Second - action user
        if (requiredMana > 0 &&
            TryComp<CP14MagicEnergyContainerComponent>(args.Performer, out var playerMana))
            _magicEnergy.ChangeEnergy((args.Performer.Value, playerMana), -requiredMana, out _, out _, safe: false);
    }

    private void OnSkillPointConsume(Entity<CP14MagicEffectSkillPointCostComponent> ent, ref CP14MagicEffectConsumeResourceEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.SkillPoint, out var indexedSkillPoint) || ent.Comp.SkillPoint is null || args.Performer is null)
            return;

        _skill.RemoveSkillPoints(args.Performer.Value, ent.Comp.SkillPoint.Value,  ent.Comp.Count);
    }
}
