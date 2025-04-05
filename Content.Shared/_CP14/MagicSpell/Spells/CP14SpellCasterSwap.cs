using Content.Shared.Examine;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;

namespace Content.Shared._CP14.MagicSpell.Spells;

public sealed partial class CP14SpellCasterSwap : CP14SpellEffect
{
    [DataField]
    public bool NeedVision = true;

    [DataField]
    public bool OnlyAlive = true;

    public override void Effect(EntityManager entManager, CP14SpellEffectBaseArgs args)
    {
        if (args.User is not { } user || args.Target is not { } target)
            return;

        var transform = entManager.System<SharedTransformSystem>();
        var userPosition = transform.GetMoverCoordinates(user);
        var targetPosition = transform.GetMoverCoordinates(target);

        var popup = entManager.System<SharedPopupSystem>();

        var mobState = entManager.System<MobStateSystem>();
        if (OnlyAlive && mobState.IsDead(target))
        {
            popup.PopupEntity(Loc.GetString("swap-ability-target-dead"), user, user);
            return;
        }

        var examine = entManager.System<ExamineSystemShared>();
        if (NeedVision && !examine.InRangeUnOccluded(user, targetPosition))
        {
            popup.PopupEntity(Loc.GetString("swap-ability-cant-see"), user, user);
            return;
        }

        transform.SetCoordinates(user, targetPosition);
        transform.SetCoordinates(target, userPosition);
    }
}
