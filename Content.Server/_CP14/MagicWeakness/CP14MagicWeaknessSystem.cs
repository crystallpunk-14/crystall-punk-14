using Content.Server.Explosion.EntitySystems;
using Content.Shared._CP14.MagicEnergy;
using Content.Shared._CP14.MagicWeakness;

namespace Content.Server._CP14.MagicWeakness;

public class CP14MagicWeaknessSystem : CP14SharedMagicWeaknessSystem
{
    [Dependency] private readonly TriggerSystem _trigger = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14MagicUnsafeTriggerComponent, CP14MagicEnergyBurnOutEvent>(OnMagicEnergyBurnOutTrigger);
        SubscribeLocalEvent<CP14MagicUnsafeTriggerComponent, CP14MagicEnergyOverloadEvent>(OnMagicEnergyOverloadTrigger);
    }

    private void OnMagicEnergyOverloadTrigger(Entity<CP14MagicUnsafeTriggerComponent> ent, ref CP14MagicEnergyOverloadEvent args)
    {
        _trigger.Trigger(ent);
    }

    private void OnMagicEnergyBurnOutTrigger(Entity<CP14MagicUnsafeTriggerComponent> ent, ref CP14MagicEnergyBurnOutEvent args)
    {
        _trigger.Trigger(ent);
    }
}
