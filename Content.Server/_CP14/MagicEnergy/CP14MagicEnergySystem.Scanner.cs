using Content.Shared._CP14.MagicEnergy.Components;
using Content.Shared.Examine;

namespace Content.Server._CP14.MagicEnergy;

public partial class CP14MagicEnergySystem
{
    private void InitializeScanner()
    {
        SubscribeLocalEvent<CP14MagicEnergyExaminableComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(Entity<CP14MagicEnergyExaminableComponent> ent, ref ExaminedEvent args)
    {
        if (!TryComp<CP14MagicEnergyContainerComponent>(ent, out var magicContainer))
            return;

        if (!args.IsInDetailsRange)
            return;

        args.PushMarkup(GetEnergyExaminedText(ent, magicContainer));
    }
}
