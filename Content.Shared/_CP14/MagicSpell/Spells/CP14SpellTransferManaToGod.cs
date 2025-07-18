using Content.Shared._CP14.MagicEnergy;
using Content.Shared._CP14.Religion.Components;
using Content.Shared._CP14.Religion.Systems;
using Content.Shared.FixedPoint;

namespace Content.Shared._CP14.MagicSpell.Spells;

public sealed partial class CP14SpellTransferManaToGod : CP14SpellEffect
{
    [DataField]
    public FixedPoint2 Amount = 10f;

    [DataField]
    public bool Safe = false;

    public override void Effect(EntityManager entManager, CP14SpellEffectBaseArgs args)
    {
        if (args.User is null)
            return;

        if (!entManager.TryGetComponent<CP14ReligionFollowerComponent>(args.User, out var follower))
            return;

        if (follower.Religion is null)
            return;

        var religionSys = entManager.System<CP14SharedReligionGodSystem>();
        var magicEnergySys = entManager.System<SharedCP14MagicEnergySystem>();

        var gods = religionSys.GetGods(follower.Religion.Value);
        var manaAmount = Amount / gods.Count;
        foreach (var god in gods)
        {
            magicEnergySys.TransferEnergy(args.User.Value, god.Owner, manaAmount, out _, out _, safe: Safe);
        }
    }
}
