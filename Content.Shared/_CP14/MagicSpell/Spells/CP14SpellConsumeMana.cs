using Content.Shared._CP14.MagicEnergy;
using Content.Shared._CP14.MagicEnergy.Components;
using Content.Shared.FixedPoint;

namespace Content.Shared._CP14.MagicSpell.Spells;

public sealed partial class CP14SpellConsumeManaEffect : CP14SpellEffect
{
    [DataField]
    public FixedPoint2 Mana = 0;

    [DataField]
    public bool Safe = false;

    public override void Effect(EntityManager entManager, CP14SpellEffectBaseArgs args)
    {
        if (args.Target is null)
            return;

        var targetEntity = args.Target.Value;

        var magicEnergy = entManager.System<SharedCP14MagicEnergySystem>();

        //First - used object
        if (args.Used is not null)
        {
            magicEnergy.TransferEnergy(targetEntity,
                args.Used.Value,
                Mana,
                out _,
                out _,
                safe: Safe);
            return;
        }

        //Second - player
        if (args.User is not null)
        {
            magicEnergy.TransferEnergy(targetEntity,
                args.User.Value,
                Mana,
                out _,
                out _,
                safe: Safe);
            return;
        }
    }
}
