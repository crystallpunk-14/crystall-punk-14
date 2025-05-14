using Content.Shared._CP14.Skill.Prototypes;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Skill.Restrictions;

public sealed partial class SpeciesWhitelist : CP14SkillRestriction
{
    [DataField(required: true)]
    public ProtoId<SpeciesPrototype> Species = new();

    public override bool Check(IEntityManager entManager, EntityUid target, CP14SkillPrototype skill)
    {
        if (!entManager.TryGetComponent<HumanoidAppearanceComponent>(target, out var appearance))
            return false;

        return appearance.Species == Species;
    }

    public override string GetDescription(IEntityManager entManager, IPrototypeManager protoManager)
    {
        var species = protoManager.Index(Species);

        return Loc.GetString("cp14-skill-req-species", ("name", Loc.GetString(species.Name)));
    }
}
