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

    private EntityQuery<CP14MagicEnergyContainerComponent> _magicContainerQuery;
    private EntityQuery<CP14MagicEffectComponent> _magicEffectQuery;

    public override void Initialize()
    {
        base.Initialize();
        InitializeActions();
        InitializeAspects();
        InitializeSlowdown();

        _magicContainerQuery = GetEntityQuery<CP14MagicEnergyContainerComponent>();
        _magicEffectQuery = GetEntityQuery<CP14MagicEffectComponent>();

        SubscribeLocalEvent<CP14MagicEffectComponent, MapInitEvent>(OnMagicEffectInit);

        SubscribeLocalEvent<CP14MagicEffectManaCostComponent, CP14CastMagicEffectAttemptEvent>(OnCheckManacost);
    }

    /// <summary>
    /// Auto generation description for spell action
    /// </summary>
    private void OnMagicEffectInit(Entity<CP14MagicEffectComponent> ent, ref MapInitEvent args)
    {
        var meta = MetaData(ent);
        var sb = new StringBuilder();

        sb.Append(meta.EntityDescription);

        if (TryComp<CP14MagicEffectManaCostComponent>(ent, out var manaCost))
        {
            sb.Append($"\n\n {Loc.GetString("cp14-magic-manacost")}: [color=#5da9e8]{manaCost.ManaCost}[/color]");
        }

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

    private void OnCheckManacost(Entity<CP14MagicEffectManaCostComponent> ent, ref CP14CastMagicEffectAttemptEvent args)
    {
        if (!_magicEffectQuery.TryComp(ent, out var magicEffect))
            return;

        var requiredMana = ent.Comp.ManaCost;

        if (ent.Comp.CanModifyManacost)
        {
            var manaEv = new CP14CalculateManacostEvent(args.Performer, ent.Comp.ManaCost, magicEffect.MagicType);

            RaiseLocalEvent(args.Performer, manaEv);

            if (magicEffect.SpellStorage is not null)
                RaiseLocalEvent(magicEffect.SpellStorage.Value, manaEv);

            requiredMana = manaEv.GetManacost();
        }

        //Try check mana from item
        if (magicEffect.SpellStorage is not null)
        {
            if (_magicContainerQuery.TryComp(magicEffect.SpellStorage, out var magicContainer))
            {
                requiredMana = MathF.Max(0, (float)(requiredMana - magicContainer.Energy));

                var spellEv = new CP14SpellFromSpellStorageUsedEvent(args.Performer, (ent, magicEffect), requiredMana);
                RaiseLocalEvent(magicEffect.SpellStorage.Value, ref spellEv);

                if (magicContainer.Energy > 0)
                {
                    //TODO: FIX THIS SHIT
                    var cashedEnergy = magicContainer.Energy;
                    _magicEnergy.TryConsumeEnergy(magicEffect.SpellStorage.Value, requiredMana, magicContainer, false);
                    requiredMana = MathF.Max(0, (float)(requiredMana - cashedEnergy));
                }
            }
        }

        //Try get mana from caster
        if (requiredMana > 0)
        {
            _magicEnergy.TryConsumeEnergy(args.Performer, requiredMana, safe: false);
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
        var fromItem = action.Comp.SpellStorage is not null;

        var doAfterEventArgs = new DoAfterArgs(EntityManager, performer, delayedEffect.CastDelay, doAfter, action, used: action.Comp.SpellStorage)
        {
            BreakOnMove = delayedEffect.BreakOnMove,
            BreakOnDamage = delayedEffect.BreakOnDamage,
            Hidden = delayedEffect.Hidden,
            DistanceThreshold = 100f,
            CancelDuplicate = true,
            BlockDuplicate = true,
            BreakOnDropItem = fromItem,
            NeedHand = fromItem,
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
}
