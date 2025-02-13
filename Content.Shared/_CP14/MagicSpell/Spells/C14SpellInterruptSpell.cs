using Content.Shared._CP14.MagicSpell.Components;
using Content.Shared.Electrocution;

namespace Content.Shared._CP14.MagicSpell.Spells;

public sealed partial class CP14SpellInterruptSpell : CP14SpellEffect
{
    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(5);

    [DataField]
    public int Damage = 10;

    public override void Effect(EntityManager entManager, CP14SpellEffectBaseArgs args)
    {
        if (args.Target is null)
            return;

        if (!entManager.TryGetComponent<CP14MagicCasterComponent>(args.Target.Value, out var caster))
            return;

        var interrupt = false;
        foreach (var spell in caster.CastedSpells)
        {
            if (entManager.HasComponent<CP14MagicEffectManaCostComponent>(spell))
            {
                interrupt = true;
                break;
            }
        }

        if (!interrupt)
            return;

        var electrocutionSystem = entManager.System<SharedElectrocutionSystem>();

        electrocutionSystem.TryDoElectrocution(args.Target.Value, args.User, Damage, Duration, true, ignoreInsulation: true );
    }
}
