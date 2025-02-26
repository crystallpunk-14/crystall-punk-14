using System.Text;
using Content.Shared.Examine;
using Content.Shared.Inventory;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicEssence;

public partial class CP14MagicEssenceSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14MagicEssenceContainerComponent, ExaminedEvent>(OnExamine);

        SubscribeLocalEvent<CP14MagicEssenceScannerComponent, CP14MagicEssenceScanEvent>(OnMagicScanAttempt);
        SubscribeLocalEvent<CP14MagicEssenceScannerComponent, InventoryRelayedEvent<CP14MagicEssenceScanEvent>>((e, c, ev) => OnMagicScanAttempt(e, c, ev.Args));
    }

    private void OnMagicScanAttempt(EntityUid uid, CP14MagicEssenceScannerComponent component, CP14MagicEssenceScanEvent args)
    {
        args.CanScan = true;
    }

    private void OnExamine(Entity<CP14MagicEssenceContainerComponent> ent, ref ExaminedEvent args)
    {
        var scanEvent = new CP14MagicEssenceScanEvent();
        RaiseLocalEvent(args.Examiner, scanEvent);

        if (!scanEvent.CanScan)
            return;

        var sb = new StringBuilder();
        sb.Append(Loc.GetString("cp14-magic-essence-title") + "\n");
        foreach (var essence in ent.Comp.Essences)
        {
            if (_proto.TryIndex(essence.Key, out var magicType))
            {
                sb.Append($"[color={magicType.Color.ToHex()}]{Loc.GetString(magicType.Name)}[/color]: x{essence.Value}\n");
            }
        }

        args.PushMarkup(sb.ToString());
    }
}

public sealed class CP14MagicEssenceScanEvent : EntityEventArgs, IInventoryRelayEvent
{
    public bool CanScan;
    public SlotFlags TargetSlots { get; } = SlotFlags.EYES;
}
