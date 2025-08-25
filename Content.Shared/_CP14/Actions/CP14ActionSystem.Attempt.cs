using System.Linq;
using Content.Shared._CP14.Actions.Components;
using Content.Shared._CP14.MagicEnergy;
using Content.Shared._CP14.MagicEnergy.Components;
using Content.Shared._CP14.MagicSpell.Components;
using Content.Shared._CP14.MagicSpell.Events;
using Content.Shared._CP14.Religion.Components;
using Content.Shared._CP14.Religion.Systems;
using Content.Shared._CP14.Skill.Components;
using Content.Shared.Actions.Events;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Damage.Components;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Speech.Muting;
using Content.Shared.SSDIndicator;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared._CP14.Actions;

public abstract partial class CP14SharedActionSystem
{
    [Dependency] private readonly SharedHandsSystem _hand = default!;
    [Dependency] private readonly CP14SharedMagicEnergySystem _magicEnergy = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly CP14SharedReligionGodSystem _god = default!;
    private void InitializeAttempts()
    {
        SubscribeLocalEvent<CP14ActionFreeHandsRequiredComponent, ActionAttemptEvent>(OnSomaticActionAttempt);
        SubscribeLocalEvent<CP14ActionSpeakingComponent, ActionAttemptEvent>(OnVerbalActionAttempt);
        SubscribeLocalEvent<CP14ActionMaterialCostComponent, ActionAttemptEvent>(OnMaterialActionAttempt);
        SubscribeLocalEvent<CP14ActionManaCostComponent, ActionAttemptEvent>(OnManacostActionAttempt);
        SubscribeLocalEvent<CP14ActionStaminaCostComponent, ActionAttemptEvent>(OnStaminaCostActionAttempt);
        SubscribeLocalEvent<CP14ActionDangerousComponent, ActionAttemptEvent>(OnDangerousActionAttempt);
        SubscribeLocalEvent<CP14ActionSkillPointCostComponent, ActionAttemptEvent>(OnSkillPointActionAttempt);

        SubscribeLocalEvent<CP14ActionSSDBlockComponent, ActionValidateEvent>(OnActionSSDAttempt);
        SubscribeLocalEvent<CP14ActionTargetMobStatusRequiredComponent, ActionValidateEvent>(OnTargetMobStatusRequiredValidate);
        SubscribeLocalEvent<CP14ActionReligionRestrictedComponent, ActionValidateEvent>(OnReligionActionValidate);
    }

    /// <summary>
    /// Before using a spell, a mana check is made for the amount of mana to show warnings.
    /// </summary>
    private void OnManacostActionAttempt(Entity<CP14ActionManaCostComponent> ent, ref ActionAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        TryComp<CP14MagicEffectComponent>(ent, out var magicEffect);

        //Total man required
        var requiredMana = ent.Comp.ManaCost;

        if (ent.Comp.CanModifyManacost)
        {
            var manaEv = new CP14CalculateManacostEvent(args.User, ent.Comp.ManaCost);

            RaiseLocalEvent(args.User, manaEv);

            if (magicEffect?.SpellStorage is not null)
                RaiseLocalEvent(magicEffect.SpellStorage.Value, manaEv);

            requiredMana = manaEv.GetManacost();
        }

        //First - trying get mana from item
        if (magicEffect is not null && magicEffect.SpellStorage is not null &&
            TryComp<CP14MagicEnergyContainerComponent>(magicEffect.SpellStorage, out var magicContainer))
            requiredMana = MathF.Max(0, (float)(requiredMana - magicContainer.Energy));

        if (requiredMana <= 0)
            return;

        //Second - trying get mana from performer
        if (!TryComp<CP14MagicEnergyContainerComponent>(args.User, out var playerMana))
        {
            Popup.PopupClient(Loc.GetString("cp14-magic-spell-no-mana-component"), args.User, args.User);
            args.Cancelled = true;
            return;
        }

        if (!_magicEnergy.HasEnergy(args.User, requiredMana, playerMana, true) && _timing.IsFirstTimePredicted)
            Popup.PopupClient(Loc.GetString($"cp14-magic-spell-not-enough-mana-cast-warning-{_random.Next(5)}"),
                args.User,
                args.User,
                PopupType.SmallCaution);
    }

    private void OnStaminaCostActionAttempt(Entity<CP14ActionStaminaCostComponent> ent, ref ActionAttemptEvent args)
    {
        if (!TryComp<StaminaComponent>(args.User, out var staminaComp))
            return;

        if (!staminaComp.Critical)
            return;

        Popup.PopupClient(Loc.GetString("cp14-magic-spell-stamina-not-enough"), args.User, args.User);
        args.Cancelled = true;
    }

    private void OnSomaticActionAttempt(Entity<CP14ActionFreeHandsRequiredComponent> ent, ref ActionAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        if (TryComp<HandsComponent>(args.User, out var hands) || hands is not null)
        {
            if (_hand.CountFreeableHands((args.User, hands)) >= ent.Comp.FreeHandRequired)
                return;
        }

        Popup.PopupClient(Loc.GetString("cp14-magic-spell-need-somatic-component"), args.User, args.User);
        args.Cancelled = true;
    }

    private void OnVerbalActionAttempt(Entity<CP14ActionSpeakingComponent> ent, ref ActionAttemptEvent args)
    {
        if (!HasComp<MutedComponent>(args.User))
            return;

        Popup.PopupClient(Loc.GetString("cp14-magic-spell-need-verbal-component"), args.User, args.User);
        args.Cancelled = true;
    }

    private void OnMaterialActionAttempt(Entity<CP14ActionMaterialCostComponent> ent, ref ActionAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        if (ent.Comp.Requirement is null)
            return;

        HashSet<EntityUid> heldedItems = new();

        foreach (var hand in _hand.EnumerateHands(args.User))
        {
            var helded = _hand.GetHeldItem(args.User, hand);
            if (helded is not null)
                heldedItems.Add(helded.Value);
        }

        if (!ent.Comp.Requirement.CheckRequirement(EntityManager, _proto, heldedItems))
        {
            Popup.PopupClient(Loc.GetString("cp14-magic-spell-need-material-component"), args.User, args.User);
            args.Cancelled = true;
        }
    }

    private void OnDangerousActionAttempt(Entity<CP14ActionDangerousComponent> ent, ref ActionAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        if (HasComp<PacifiedComponent>(args.User))
        {
            Popup.PopupClient(Loc.GetString("cp14-magic-spell-pacified"), args.User, args.User);
            args.Cancelled = true;
        }
    }

    private void OnSkillPointActionAttempt(Entity<CP14ActionSkillPointCostComponent> ent, ref ActionAttemptEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.SkillPoint, out var indexedSkillPoint) || ent.Comp.SkillPoint is null)
            return;

        if (!TryComp<CP14SkillStorageComponent>(args.User, out var skillStorage))
        {
            Popup.PopupClient(Loc.GetString("cp14-magic-spell-skillpoint-not-enough",
                    ("name", Loc.GetString(indexedSkillPoint.Name)),
                    ("count", ent.Comp.Count)),
                args.User,
                args.User);
            args.Cancelled = true;
            return;
        }

        var points = skillStorage.SkillPoints;
        if (points.TryGetValue(ent.Comp.SkillPoint.Value, out var currentPoints))
        {
            var freePoints = currentPoints.Max - currentPoints.Sum;

            if (freePoints < ent.Comp.Count)
            {
                var d = ent.Comp.Count - freePoints;

                Popup.PopupClient(Loc.GetString("cp14-magic-spell-skillpoint-not-enough",
                        ("name", Loc.GetString(indexedSkillPoint.Name)),
                        ("count", d)),
                    args.User,
                    args.User);
                args.Cancelled = true;
            }
        }
    }

    private void OnTargetMobStatusRequiredValidate(Entity<CP14ActionTargetMobStatusRequiredComponent> ent,
        ref ActionValidateEvent args)
    {
        if (args.Invalid)
            return;

        var target = GetEntity(args.Input.EntityTarget);

        if (!TryComp<MobStateComponent>(target, out var mobStateComp))
        {
            Popup.PopupClient(Loc.GetString("cp14-magic-spell-target-not-mob"), args.User, args.User);
            args.Invalid = true;
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

            Popup.PopupClient(Loc.GetString("cp14-magic-spell-target-mob-state", ("state", states)),
                args.User,
                args.User);
            args.Invalid = true;
        }
    }

    private void OnActionSSDAttempt(Entity<CP14ActionSSDBlockComponent> ent, ref ActionValidateEvent args)
    {
        if (args.Invalid)
            return;

        if (!TryComp<SSDIndicatorComponent>(GetEntity(args.Input.EntityTarget), out var ssdIndication))
            return;

        if (ssdIndication.IsSSD)
        {
            Popup.PopupClient(Loc.GetString("cp14-magic-spell-ssd"), args.User, args.User);
            args.Invalid = true;
        }
    }

    private void OnReligionActionValidate(Entity<CP14ActionReligionRestrictedComponent> ent, ref ActionValidateEvent args)
    {
        if (args.Invalid)
            return;

        if (!TryComp<CP14ReligionEntityComponent>(args.User, out var religionComp))
            return;

        var position = GetCoordinates(args.Input.EntityCoordinatesTarget);
        var target = GetEntity(args.Input.EntityTarget);

        if (target is not null)
            position ??= Transform(target.Value).Coordinates;

        if (ent.Comp.OnlyInReligionZone)
        {
            if (position is null || !_god.InVision(position.Value, (args.User, religionComp)))
            {
                args.Invalid = true;
            }
        }

        if (ent.Comp.OnlyOnFollowers)
        {
            if (target is null || !TryComp<CP14ReligionFollowerComponent>(target, out var follower) || follower.Religion != religionComp.Religion)
            {
                Popup.PopupClient(Loc.GetString("cp14-magic-spell-target-god-follower"), args.User, args.User);
                args.Invalid = true;
            }
        }
    }
}
