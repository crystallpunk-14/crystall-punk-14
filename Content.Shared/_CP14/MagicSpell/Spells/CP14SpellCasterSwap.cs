namespace Content.Shared._CP14.MagicSpell.Spells;

public sealed partial class CP14SpellCasterSwap : CP14SpellEffect
{
    public override void Effect(EntityManager entManager, CP14SpellEffectBaseArgs args)
    {
        if (args.User is not { } user || args.Target is not { } target)
            return;

        var transform = entManager.System<SharedTransformSystem>();
        var userPosition = transform.GetMoverCoordinates(user);
        var targetPosition = transform.GetMoverCoordinates(target);

        transform.SetCoordinates(user, targetPosition);
        transform.SetCoordinates(target, userPosition);
    }
}
