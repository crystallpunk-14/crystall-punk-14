using Content.Shared._CP14.Demiplane;
using Content.Shared._CP14.Demiplane.Components;

namespace Content.Shared._CP14.MagicSpell.Spells;

public sealed partial class CP14SpellDemiplaneInfiltration : CP14SpellEffect
{
    public override void Effect(EntityManager entManager, CP14SpellEffectBaseArgs args)
    {
        if (args.User is null)
            return;

        if (!entManager.TryGetComponent<CP14DemiplaneRiftComponent>(args.Target, out var rift))
            return;

        if (rift.Demiplane is null)
            return;

        if (!entManager.TryGetComponent<CP14DemiplaneComponent>(rift.Demiplane.Value, out var demiplane))
            return;

        var demiplaneSystem = entManager.System<CP14SharedDemiplaneSystem>();

        demiplaneSystem.TryTeleportIntoDemiplane((rift.Demiplane.Value, demiplane), args.User.Value);
    }
}
