using System.Linq;
using Content.Shared._CP14.MagicSpell.Components;
using Content.Shared.Examine;
using Content.Shared.Mobs;

namespace Content.Shared._CP14.MagicSpell;

public abstract partial class CP14SharedMagicSystem
{
    private void InitializeExamine()
    {
        SubscribeLocalEvent<CP14MagicEffectComponent, ExaminedEvent>(OnManaEffectExamined);

        SubscribeLocalEvent<CP14MagicEffectManaCostComponent, ExaminedEvent>(OnManacostExamined);
        SubscribeLocalEvent<CP14MagicEffectStaminaCostComponent, ExaminedEvent>(OnStaminaCostExamined);
        SubscribeLocalEvent<CP14MagicEffectSkillPointCostComponent, ExaminedEvent>(OnSkillPointCostExamined);

        SubscribeLocalEvent<CP14MagicEffectVerbalAspectComponent, ExaminedEvent>(OnVerbalExamined);
        SubscribeLocalEvent<CP14MagicEffectSomaticAspectComponent, ExaminedEvent>(OnSomaticExamined);
        SubscribeLocalEvent<CP14MagicEffectMaterialAspectComponent, ExaminedEvent>(OnMaterialExamined);
        SubscribeLocalEvent<CP14MagicEffectRequiredMusicToolComponent, ExaminedEvent>(OnMusicExamined);
        SubscribeLocalEvent<CP14MagicEffectTargetMobStatusRequiredComponent, ExaminedEvent>(OnMobStateExamined);
    }

    private void OnManaEffectExamined(Entity<CP14MagicEffectComponent> ent, ref ExaminedEvent args)
    {
        if (_proto.TryIndex(ent.Comp.MagicType, out var indexedMagic))
        {
            args.PushMarkup($"{Loc.GetString("cp14-magic-type")}: [color={indexedMagic.Color.ToHex()}]{Loc.GetString(indexedMagic.Name)}[/color]", 10);
        }
    }

    private void OnManacostExamined(Entity<CP14MagicEffectManaCostComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup($"{Loc.GetString("cp14-magic-manacost")}: [color=#5da9e8]{ent.Comp.ManaCost}[/color]", priority: 9);
    }

    private void OnStaminaCostExamined(Entity<CP14MagicEffectStaminaCostComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup($"{Loc.GetString("cp14-magic-staminacost")}: [color=#3fba54]{ent.Comp.Stamina}[/color]", priority: 9);
    }

    private void OnSkillPointCostExamined(Entity<CP14MagicEffectSkillPointCostComponent> ent, ref ExaminedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.SkillPoint, out var indexedSkillPoint))
            return;

        args.PushMarkup($"{Loc.GetString("cp14-magic-skillpointcost", ("name", Loc.GetString(indexedSkillPoint.Name)), ("count", ent.Comp.Count))}", priority: 9);
    }

    private void OnVerbalExamined(Entity<CP14MagicEffectVerbalAspectComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("cp14-magic-verbal-aspect"), 8);
    }

    private void OnSomaticExamined(Entity<CP14MagicEffectSomaticAspectComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("cp14-magic-somatic-aspect") + " " + ent.Comp.FreeHandRequired, 8);
    }

    private void OnMaterialExamined(Entity<CP14MagicEffectMaterialAspectComponent> ent, ref ExaminedEvent args)
    {
        if (ent.Comp.Requirement is not null)
            args.PushMarkup(Loc.GetString("cp14-magic-material-aspect") + " " + ent.Comp.Requirement.GetRequirementTitle(_proto));
    }
    private void OnMusicExamined(Entity<CP14MagicEffectRequiredMusicToolComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("cp14-magic-music-aspect"));
    }

    private void OnMobStateExamined(Entity<CP14MagicEffectTargetMobStatusRequiredComponent> ent, ref ExaminedEvent args)
    {
        var states = string.Join(", ",
            ent.Comp.AllowedStates.Select(state => state switch
        {
            MobState.Alive => Loc.GetString("cp14-magic-spell-target-mob-state-live"),
            MobState.Dead => Loc.GetString("cp14-magic-spell-target-mob-state-dead"),
            MobState.Critical => Loc.GetString("cp14-magic-spell-target-mob-state-critical")
        }));

        args.PushMarkup(Loc.GetString("cp14-magic-spell-target-mob-state", ("state", states)));
    }
}
