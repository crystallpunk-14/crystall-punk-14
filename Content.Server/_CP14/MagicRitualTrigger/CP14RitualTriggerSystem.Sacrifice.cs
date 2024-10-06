using Content.Shared._CP14.MagicRitual;
using Content.Shared._CP14.MagicRitualTrigger.Triggers;
using Content.Shared.Humanoid;
using Content.Shared.Mobs;

namespace Content.Server._CP14.MagicRitualTrigger;


public partial class CP14RitualTriggerSystem
{
    private void InitializeSacrifise()
    {
        SubscribeLocalEvent<MobStateChangedEvent>(OnMobStateChanged);
    }

    private void OnMobStateChanged(MobStateChangedEvent ev)
    {
        if (ev.NewMobState != MobState.Dead)
            return;

        if (!TryComp<HumanoidAppearanceComponent>(ev.Target, out var humanoid))
            return;

        var deathXform = Transform(ev.Target);

        var query = EntityQueryEnumerator<CP14RitualSacrificeSpeciesTriggerComponent, CP14MagicRitualPhaseComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var sacrifice, out var phase, out var xform))
        {
            if (!deathXform.Coordinates.TryDistance(EntityManager, xform.Coordinates, out var distance))
                continue;

            foreach (var trigger in sacrifice.Triggers)
            {
                if (distance > trigger.Range)
                    continue;

                if (trigger.Edge is null)
                    continue;

                if (trigger.Species != humanoid.Species)
                    continue;

                TriggerRitualPhase((uid, phase), trigger.Edge.Value.Target);
            }
        }
    }
}
