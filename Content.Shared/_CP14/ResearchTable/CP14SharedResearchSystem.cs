using System.Text;
using Content.Shared._CP14.Skill;
using Content.Shared._CP14.Skill.Components;
using Content.Shared._CP14.Skill.Prototypes;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Inventory;
using Content.Shared.Verbs;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.ResearchTable;

public abstract class CP14SharedResearchSystem : EntitySystem
{
    [Dependency] private readonly ExamineSystemShared _examine = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly CP14SharedSkillSystem _skill = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14SkillScannerComponent, CP14SkillScanEvent>(OnSkillScan);
        SubscribeLocalEvent<CP14SkillScannerComponent, InventoryRelayedEvent<CP14SkillScanEvent>>((e, c, ev) => OnSkillScan(e, c, ev.Args));

        SubscribeLocalEvent<CP14SkillStorageComponent, GetVerbsEvent<ExamineVerb>>(OnExamined);
    }

    private void OnExamined(Entity<CP14SkillStorageComponent> ent, ref GetVerbsEvent<ExamineVerb> args)
    {
        var scanEvent = new CP14SkillScanEvent();
        RaiseLocalEvent(args.User, scanEvent);

        if (!scanEvent.CanScan)
            return;

        var markup = GetSkillExamine(ent);

        _examine.AddDetailedExamineVerb(
            args,
            ent.Comp,
            markup,
            Loc.GetString("cp14-skill-examine"),
            "/Textures/Interface/students-cap.svg.192dpi.png");
    }

    private FormattedMessage GetSkillExamine(Entity<CP14SkillStorageComponent> ent)
    {
        var msg = new FormattedMessage();

        var sb = new StringBuilder();

        sb.Append(Loc.GetString("cp14-skill-examine-title") + "\n");

        foreach (var skill in ent.Comp.LearnedSkills)
        {
            if (!_proto.TryIndex(skill, out var indexedSkill))
                continue;

            if(!_proto.TryIndex(indexedSkill.Tree, out var indexedTree))
                continue;

            var skillName = _skill.GetSkillName(skill);
            sb.Append($"• [color={indexedTree.Color.ToHex()}]{skillName}[/color]\n");
        }

        sb.Append($"\n{Loc.GetString("cp14-skill-menu-level")} {ent.Comp.SkillsSumExperience}/{ent.Comp.ExperienceMaxCap}\n");
        msg.AddMarkupOrThrow(sb.ToString());
        return msg;
    }

    private void OnSkillScan(EntityUid uid, CP14SkillScannerComponent component, CP14SkillScanEvent args)
    {
        args.CanScan = true;
    }
}

[Serializable, NetSerializable]
public sealed partial class CP14ResearchDoAfterEvent : DoAfterEvent
{
    [DataField(required: true)]
    public ProtoId<CP14SkillPrototype> Skill = default!;

    public override DoAfterEvent Clone() => this;
}

public sealed class CP14SkillScanEvent : EntityEventArgs, IInventoryRelayEvent
{
    public bool CanScan;
    public SlotFlags TargetSlots { get; } = SlotFlags.EYES;
}
