using Content.Client.Items;
using Content.Shared._CP14.MagicEnergy;
using Content.Shared._CP14.MagicEnergy.Components;

namespace Content.Client._CP14.MagicEnergy;

public sealed class CP14MagicEnergySystem : CP14SharedMagicEnergySystem
{
    public override void Initialize()
    {
        base.Initialize();

        Subs.ItemStatus<CP14MagicEnergyExaminableComponent>( ent => new CP14MagicEnergyStatusControl(ent));
    }
}
