using System.Diagnostics.CodeAnalysis;
using Content.Shared._CP14.Magic.Events;
using Content.Shared.DoAfter;
using Content.Shared.Hands.Components;
using Content.Shared.Verbs;

namespace Content.Server._CP14.Magic;

public sealed partial class CPMagicSpellContainerSystem
{
    private void InitializeCast()
    {
        SubscribeLocalEvent<CPMagicSpellContainerComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerb);
        SubscribeLocalEvent<CPMagicSpellContainerComponent, CPMagicCastDoAfterEvent>(OnCast);
    }

    private void OnGetVerb(Entity<CPMagicSpellContainerComponent> container, ref GetVerbsEvent<AlternativeVerb> args)
    {
        var user = args.User;

        args.Verbs.Add(new AlternativeVerb
        {
            Text = "Get spell",
            Disabled = container.Comp.Effects.Count == 0,
            Priority = 10,
            Act = () =>
            {
                StartCast(container, user);
            }
        });
    }

    private void OnCast(Entity<CPMagicSpellContainerComponent> container, ref CPMagicCastDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Target is null)
            return;

        Cast(container, args.Target.Value);
        args.Handled = true;
    }

    public void StartCast(Entity<CPMagicSpellContainerComponent> container, EntityUid caster)
    {
        if (!CastValidate(container, caster, out _))
            return;

        var doAfterArgs = new DoAfterArgs(EntityManager, caster, container.Comp.TotalCastTime, new CPMagicCastDoAfterEvent(), target: caster, used: container, eventTarget: container)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            MovementThreshold = 0.5f,
            CancelDuplicate = false,
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    public void Cast(Entity<CPMagicSpellContainerComponent> container, EntityUid caster)
    {
        if (!CastValidate(container, caster, out var hand))
            return;

        var entity = Spawn(BaseSpellItemEntity, _transform.GetMapCoordinates(container));
        var spell = EnsureComp<CPMagicSpellComponent>(entity);

        spell.Effects =  new List<CPMagicEffectPrototype>(container.Comp.EffectPrototypes);

        _hands.TryPickup(caster, entity, hand);
    }

    private bool CastValidate(Entity<CPMagicSpellContainerComponent> container, EntityUid caster, [NotNullWhen(true)] out Hand? hand)
    {
        hand = default;

        if (container.Comp.TotalCompleteness > container.Comp.MaximumCompleteness)
        {
            _popup.PopupEntity("Too much complicated", container);
            return false;
        }

        if (!_hands.TryGetEmptyHand(caster, out hand))
        {
            _popup.PopupEntity("The spell can't fit in your hand", container);
            return false;
        }

        return true;
    }
}
