using Content.Shared._CP14.Action.Components;
using Content.Shared._CP14.MagicEnergy.Components;
using Content.Shared._CP14.MagicSpell.Components;
using Content.Shared._CP14.MagicSpell.Events;
using Content.Shared._CP14.Religion.Systems;
using Content.Shared._CP14.Skill;
using Content.Shared.FixedPoint;
using Content.Shared.Hands.EntitySystems;

namespace Content.Shared._CP14.MagicSpell;

public abstract partial class CP14SharedMagicSystem
{
    [Dependency] private readonly CP14SharedReligionGodSystem _god = default!;
    [Dependency] private readonly SharedHandsSystem _hand = default!;
    [Dependency] private readonly CP14SharedSkillSystem _skill = default!;

    private void InitializeChecks()
    {
        //Verbal speaking
        SubscribeLocalEvent<CP14ActionSpeakingComponent, CP14StartCastMagicEffectEvent>(OnVerbalAspectStartCast);
        SubscribeLocalEvent<CP14ActionSpeakingComponent, CP14MagicEffectConsumeResourceEvent>(OnVerbalAspectAfterCast);

        SubscribeLocalEvent<CP14MagicEffectEmotingComponent, CP14StartCastMagicEffectEvent>(OnEmoteStartCast);
        SubscribeLocalEvent<CP14MagicEffectEmotingComponent, CP14MagicEffectConsumeResourceEvent>(OnEmoteEndCast);

        //Consuming resources
        SubscribeLocalEvent<CP14ActionMaterialCostComponent, CP14MagicEffectConsumeResourceEvent>(OnMaterialAspectEndCast);
        SubscribeLocalEvent<CP14ActionStaminaCostComponent, CP14MagicEffectConsumeResourceEvent>(OnStaminaConsume);
        SubscribeLocalEvent<CP14ActionManaCostComponent, CP14MagicEffectConsumeResourceEvent>(OnManaConsume);
        SubscribeLocalEvent<CP14ActionSkillPointCostComponent, CP14MagicEffectConsumeResourceEvent>(OnSkillPointConsume);
    }

    private void OnVerbalAspectStartCast(Entity<CP14ActionSpeakingComponent> ent,
        ref CP14StartCastMagicEffectEvent args)
    {
        var ev = new CP14ActionSpeechEvent
        {
            Performer = args.Performer,
            Speech = Loc.GetString(ent.Comp.StartSpeech),
            Emote = false,
        };
        RaiseLocalEvent(ent, ref ev);
    }

    private void OnVerbalAspectAfterCast(Entity<CP14ActionSpeakingComponent> ent,
        ref CP14MagicEffectConsumeResourceEvent args)
    {
        var ev = new CP14ActionSpeechEvent
        {
            Performer = args.Performer,
            Speech = Loc.GetString(ent.Comp.EndSpeech),
            Emote = false
        };
        RaiseLocalEvent(ent, ref ev);
    }

    private void OnEmoteStartCast(Entity<CP14MagicEffectEmotingComponent> ent, ref CP14StartCastMagicEffectEvent args)
    {
        var ev = new CP14ActionSpeechEvent
        {
            Performer = args.Performer,
            Speech = Loc.GetString(ent.Comp.StartEmote),
            Emote = true,
        };
        RaiseLocalEvent(ent, ref ev);
    }


    private void OnEmoteEndCast(Entity<CP14MagicEffectEmotingComponent> ent, ref CP14MagicEffectConsumeResourceEvent args)
    {
        var ev = new CP14ActionSpeechEvent
        {
            Performer = args.Performer,
            Speech = Loc.GetString(ent.Comp.EndEmote),
            Emote = true
        };
        RaiseLocalEvent(ent, ref ev);
    }

    private void OnMaterialAspectEndCast(Entity<CP14ActionMaterialCostComponent> ent, ref CP14MagicEffectConsumeResourceEvent args)
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

    private void OnStaminaConsume(Entity<CP14ActionStaminaCostComponent> ent, ref CP14MagicEffectConsumeResourceEvent args)
    {
        if (args.Performer is null)
            return;

        _stamina.TakeStaminaDamage(args.Performer.Value, ent.Comp.Stamina, visual: false);
    }

    private void OnManaConsume(Entity<CP14ActionManaCostComponent> ent, ref CP14MagicEffectConsumeResourceEvent args)
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

    private void OnSkillPointConsume(Entity<CP14ActionSkillPointCostComponent> ent, ref CP14MagicEffectConsumeResourceEvent args)
    {
        if (ent.Comp.SkillPoint is null || args.Performer is null)
            return;

        _skill.RemoveSkillPoints(args.Performer.Value, ent.Comp.SkillPoint.Value,  ent.Comp.Count);
    }
}
