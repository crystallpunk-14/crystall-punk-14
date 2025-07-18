using Content.Shared._CP14.Religion.Components;
using Content.Shared._CP14.Religion.Systems;

namespace Content.Shared._CP14.MagicSpell.Spells;

public sealed partial class CP14SpellGodRenounce : CP14SpellEffect
{
    public override void Effect(EntityManager entManager, CP14SpellEffectBaseArgs args)
    {
        if (args.Target is null)
            return;

        if (!entManager.TryGetComponent<CP14ReligionEntityComponent>(args.User, out var god) || god.Religion is null)
            return;

        if (!entManager.TryGetComponent<CP14ReligionFollowerComponent>(args.Target.Value, out var follower) || follower.Religion != god.Religion)
            return;

        var religionSys = entManager.System<CP14SharedReligionGodSystem>();

        religionSys.ToDisbelieve(args.Target.Value);
    }
}
