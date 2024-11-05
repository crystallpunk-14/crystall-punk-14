
using Content.Shared._CP14.MagicRitual.Prototypes;
using Content.Shared._CP14.MagicSpell.Events;
using Content.Shared.Examine;
using Content.Shared.FixedPoint;
using Content.Shared.Inventory;
using Content.Shared.Verbs;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.MagicClothing;

public sealed partial class CP14MagicClothingSystem : EntitySystem
{
    [Dependency] private readonly ExamineSystemShared _examine = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14MagicClothingManacostModifyComponent, InventoryRelayedEvent<CP14CalculateManacostEvent>>(OnCalculateManacost);
        SubscribeLocalEvent<CP14MagicClothingManacostModifyComponent, GetVerbsEvent<ExamineVerb>>(OnVerbExamine);
    }

    private void OnVerbExamine(Entity<CP14MagicClothingManacostModifyComponent> ent, ref GetVerbsEvent<ExamineVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess)
            return;

        var markup = GetMagicClothingExamine(ent.Comp);
        _examine.AddDetailedExamineVerb(
            args,
            ent.Comp,
            markup,
            Loc.GetString("armor-examinable-verb-text"),
            "/Textures/Interface/VerbIcons/dot.svg.192dpi.png",
            Loc.GetString("armor-examinable-verb-message"));
    }

    private FormattedMessage GetMagicClothingExamine(CP14MagicClothingManacostModifyComponent comp)
    {
        var msg = new FormattedMessage();
        msg.AddMarkupOrThrow(Loc.GetString("cp14-clothing-magic-examine"));

        if (comp.GlobalModifier != 1)
        {
            msg.PushNewline();

            var plus = (float)comp.GlobalModifier > 1 ? "+" : "";
            msg.AddMarkupOrThrow($"{Loc.GetString("cp14-clothing-magic-global")}: {plus}{((float)comp.GlobalModifier - 1)*100}%");
        }

        foreach (var modifier in comp.Modifiers)
        {
            if (modifier.Value == 1)
                continue;

            msg.PushNewline();

            var plus = modifier.Value > 1 ? "+" : "";
            var indexedType = _proto.Index(modifier.Key);
            msg.AddMarkupOrThrow($"[color={indexedType.Color.ToHex()}]{Loc.GetString(indexedType.Name)}[/color]: {plus}{(modifier.Value - 1)*100}%");
        }

        return msg;
    }

    private void OnCalculateManacost(Entity<CP14MagicClothingManacostModifyComponent> ent, ref InventoryRelayedEvent<CP14CalculateManacostEvent> args)
    {
        args.Args.Multiplier += (float)ent.Comp.GlobalModifier;

        if (ent.Comp.Modifiers.TryGetValue(args.Args.MagicType, out var modifier))
        {
            args.Args.Multiplier *= (float)modifier;
        }
    }
}
