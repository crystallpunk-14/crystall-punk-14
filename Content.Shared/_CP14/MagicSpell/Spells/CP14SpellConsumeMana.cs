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
            magicEnergy.TransferEnergy(targetEntity, args.Used.Value, Mana, out var transferedEnergy, out var overloadedEnergy, safe: Safe);

            var diff = Mana - (transferedEnergy + overloadedEnergy);
            if (diff > 0 && args.User is not null)
            {
                magicEnergy.ChangeEnergy(args.User.Value, diff, out _, out _, safe: Safe);
            }
        }
    }
}
