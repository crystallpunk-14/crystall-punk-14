using Content.Shared._CP14.Magic.Components;
using Content.Shared._CP14.Magic.Events;
using Content.Shared._CP14.MagicEnergy;
using Content.Shared._CP14.MagicEnergy.Components;
using Content.Shared.DoAfter;
using Content.Shared.Popups;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Random;

namespace Content.Shared._CP14.Magic;

public sealed partial class CP14SharedMagicSystem : EntitySystem
{
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly SharedGunSystem _gunSystem = default!;
    [Dependency] private readonly SharedCP14MagicEnergySystem _magicEnergy = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14MagicEffectComponent, CP14BeforeCastMagicEffectEvent>(OnBeforeCastMagicEffect);
        SubscribeLocalEvent<CP14MagicEffectComponent, CP14AfterCastMagicEffectEvent>(OnAfterCastMagicEffect);

        SubscribeLocalEvent<CP14DelayedInstantActionEvent>(OnInstantAction);
        SubscribeLocalEvent<CP14DelayedEntityTargetActionEvent>(OnEntityTargetAction);
        SubscribeLocalEvent<CP14DelayedWorldTargetActionEvent>(OnWorldTargetAction);

        InitializeSpells();
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
        };

        _doAfter.TryStartDoAfter(doAfterEventArgs);
    }

    private void OnWorldTargetAction(CP14DelayedWorldTargetActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        if (args is not ICP14DelayedMagicEffect delayedEffect)
            return;

        if (!TryCastSpell(args.Action, args.Performer))
            return;

        var doAfter = new CP14DelayedWorldTargetActionDoAfterEvent()
        {
            Target = EntityManager.GetNetCoordinates(args.Target)
        };

        var doAfterEventArgs = new DoAfterArgs(EntityManager, args.Performer, delayedEffect.Delay, doAfter, args.Action)
        {
            BreakOnMove = delayedEffect.BreakOnMove,
            BreakOnDamage = delayedEffect.BreakOnDamage,
        };

        _doAfter.TryStartDoAfter(doAfterEventArgs);
    }

    private void OnEntityTargetAction(CP14DelayedEntityTargetActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        if (args is not ICP14DelayedMagicEffect delayedEffect)
            return;

        if (!TryCastSpell(args.Action, args.Performer))
            return;

        var doAfterEventArgs = new DoAfterArgs(EntityManager, args.Performer, delayedEffect.Delay, new CP14DelayedEntityTargetActionDoAfterEvent(), args.Action, args.Target)
        {
            BreakOnMove = delayedEffect.BreakOnMove,
            BreakOnDamage = delayedEffect.BreakOnDamage,
        };

        _doAfter.TryStartDoAfter(doAfterEventArgs);
    }

    private bool TryCastSpell(EntityUid spell, EntityUid performer)
    {
        var ev = new CP14BeforeCastMagicEffectEvent()
        {
            Permormer = performer,
        };
        RaiseLocalEvent(spell, ref ev);
        return !ev.Cancelled;
    }

    private void OnBeforeCastMagicEffect(Entity<CP14MagicEffectComponent> ent, ref CP14BeforeCastMagicEffectEvent args)
    {
        if (!TryComp<CP14MagicEnergyContainerComponent>(args.Permormer, out var magicContainer))
        {
            args.Cancel();
            return;
        }

        if (!_magicEnergy.HasEnergy(args.Permormer.Value, ent.Comp.ManaCost, safe: ent.Comp.Safe))
        {
            args.Cancel();
            _popup.PopupEntity(Loc.GetString("cp14-magic-spell-not-enough-mana"), args.Permormer.Value, args.Permormer.Value);
        }
        else if(!_magicEnergy.HasEnergy(args.Permormer.Value, ent.Comp.ManaCost, safe: true) && _net.IsServer)
        {
            _popup.PopupEntity(Loc.GetString("cp14-magic-spell-not-enough-mana-cast-warning-"+_random.Next(5)), args.Permormer.Value, args.Permormer.Value, PopupType.SmallCaution);
        }
    }

    private void OnAfterCastMagicEffect(Entity<CP14MagicEffectComponent> ent, ref CP14AfterCastMagicEffectEvent args)
    {
        if (_net.IsClient)
            return;

        if (!HasComp<CP14MagicEnergyContainerComponent>(args.Permormer))
            return;

        _magicEnergy.TryConsumeEnergy(args.Permormer.Value, ent.Comp.ManaCost, safe: ent.Comp.Safe);
    }
}
