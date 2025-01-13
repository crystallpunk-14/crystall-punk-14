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

    [DataField]
    public float LossMultiplier = 1.0f;

    public override void Effect(EntityManager entManager, CP14SpellEffectBaseArgs args)
    {
        if (args.Target is null)
            return;

        var targetEntity = args.Target.Value;

        if (!entManager.TryGetComponent<CP14MagicEnergyContainerComponent>(targetEntity, out var magicContainer))
            return;

        var magicEnergy = entManager.System<SharedCP14MagicEnergySystem>();

        var currentMana = magicContainer.Energy;
        FixedPoint2 manaBuffer = MathF.Min(Mana.Float(), currentMana.Float())  * LossMultiplier;

        if (!magicEnergy.TryConsumeEnergy(targetEntity, Mana, magicContainer, Safe))
            return;

        //OK, we consume mana (or health?) from target, and now we put it in used object or caster


        //First - used object
        if (manaBuffer > 0 && entManager.TryGetComponent<CP14MagicEnergyContainerComponent>(args.Used, out var usedMagicStorage))
        {
            var freeSpace = usedMagicStorage.MaxEnergy - usedMagicStorage.Energy;
            if (freeSpace < manaBuffer)
            {
                magicEnergy.ChangeEnergy(args.Used.Value, usedMagicStorage, freeSpace, true);
                manaBuffer -= freeSpace;
            }
            else
            {
                magicEnergy.ChangeEnergy(args.Used.Value, usedMagicStorage, manaBuffer, true);
                manaBuffer = 0;
            }
        }

        //Second - action user
        if (manaBuffer > 0 && entManager.TryGetComponent<CP14MagicEnergyContainerComponent>(args.User, out var userMagicStorage))
        {
            magicEnergy.ChangeEnergy(args.User.Value, userMagicStorage, manaBuffer, Safe);
        }
    }
}
