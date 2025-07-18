using Content.Shared._CP14.Dash;

namespace Content.Shared._CP14.MagicSpell.Spells;

public sealed partial class CP14SpellDash : CP14SpellEffect
{
    [DataField]
    public float Speed = 10f;

    [DataField]
    public float Range = 3.5f;

    public override void Effect(EntityManager entManager, CP14SpellEffectBaseArgs args)
    {
        if (args.User is null)
            return;
        if (args.Position is null)
            return;

        var dashSys = entManager.System<CP14DashSystem>();

        dashSys.PerformDash(args.User.Value, args.Position.Value, Speed, Range);
    }
}
