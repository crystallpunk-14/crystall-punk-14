using Content.Shared.Examine;
using Content.Shared.Popups;
using Robust.Shared.Map;

namespace Content.Shared._CP14.MagicSpell.Spells;

public sealed partial class CP14SpellCasterTeleport : CP14SpellEffect
{
    [DataField]
    public bool NeedVision = true;

    public override void Effect(EntityManager entManager, CP14SpellEffectBaseArgs args)
    {
        EntityCoordinates? targetPoint = null;
        if (args.Position is not null)
            targetPoint = args.Position.Value;
        else if (args.Target is not null && entManager.TryGetComponent<TransformComponent>(args.Target.Value, out var transformComponent))
            targetPoint = transformComponent.Coordinates;

        if (targetPoint is null || args.User is null)
            return;

        var transform = entManager.System<SharedTransformSystem>();
        var examine = entManager.System<ExamineSystemShared>();
        var popup = entManager.System<SharedPopupSystem>();

        if (NeedVision && !examine.InRangeUnOccluded(args.User.Value, targetPoint.Value))
        {
            // can only dash if the destination is visible on screen
            popup.PopupEntity(Loc.GetString("dash-ability-cant-see"), args.User.Value, args.User.Value);
            return;
        }

        transform.SetCoordinates(args.User.Value, targetPoint.Value);
    }
}
