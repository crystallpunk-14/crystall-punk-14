
using Content.Shared._CP14.MagicRitual.Prototypes;
using Content.Shared._CP14.MagicSpell.Events;
using Content.Shared.Examine;
using Content.Shared.FixedPoint;
using Content.Shared.Inventory;
using Content.Shared.Verbs;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.MagicManacostModify;

public sealed partial class CP14MagicManacostModifySystem : EntitySystem
{
    [Dependency] private readonly ExamineSystemShared _examine = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14MagicManacostModifyComponent, InventoryRelayedEvent<CP14CalculateManacostEvent>>(OnCalculateManacost);
        SubscribeLocalEvent<CP14MagicManacostModifyComponent, CP14CalculateManacostEvent>(OnCalculateManacost);
        SubscribeLocalEvent<CP14MagicManacostModifyComponent, GetVerbsEvent<ExamineVerb>>(OnVerbExamine);
    }

    private void OnVerbExamine(Entity<CP14MagicManacostModifyComponent> ent, ref GetVerbsEvent<ExamineVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess || !ent.Comp.Examinable)
            return;

        var markup = GetManacostModifyMessage(ent.Comp.GlobalModifier, ent.Comp.Modifiers);
        _examine.AddDetailedExamineVerb(
            args,
            ent.Comp,
            markup,
            Loc.GetString("cp14-magic-examinable-verb-text"),
            "/Textures/Interface/VerbIcons/bubbles.svg.192dpi.png",
            Loc.GetString("cp14-magic-examinable-verb-message"));
    }

    public FormattedMessage GetManacostModifyMessage(FixedPoint2 global, Dictionary<ProtoId<CP14MagicTypePrototype>, FixedPoint2> modifiers)
    {
        var msg = new FormattedMessage();
        msg.AddMarkupOrThrow(Loc.GetString("cp14-clothing-magic-examine"));

        if (global != 1)
        {
            msg.PushNewline();

            var plus = (float)global > 1 ? "+" : "";
            msg.AddMarkupOrThrow(
                $"{Loc.GetString("cp14-clothing-magic-global")}: {plus}{MathF.Round((float)(global - 1) * 100, MidpointRounding.AwayFromZero)}%");
        }

        foreach (var modifier in modifiers)
        {
            if (modifier.Value == 1)
                continue;

            msg.PushNewline();

            var plus = modifier.Value > 1 ? "+" : "";
            var indexedType = _proto.Index(modifier.Key);
            msg.AddMarkupOrThrow($"- [color={indexedType.Color.ToHex()}]{Loc.GetString(indexedType.Name)}[/color]: {plus}{(modifier.Value - 1)*100}%");
        }

        return msg;
    }

    private void OnCalculateManacost(Entity<CP14MagicManacostModifyComponent> ent, ref InventoryRelayedEvent<CP14CalculateManacostEvent> args)
    {
        OnCalculateManacost(ent, ref args.Args);
    }

    private void OnCalculateManacost(Entity<CP14MagicManacostModifyComponent> ent, ref CP14CalculateManacostEvent args)
    {
        args.Multiplier *= (float)ent.Comp.GlobalModifier;

        if (args.MagicType is not null && ent.Comp.Modifiers.TryGetValue(args.MagicType.Value, out var modifier))
        {
            args.Multiplier *= (float)modifier;
        }
    }
}
