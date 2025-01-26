using System.Text;
using Content.Shared._CP14.MagicRitual.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicRitual.Requirements;

/// <summary>
/// Requires specific daytime period
/// </summary>
public sealed partial class RequiredOrbs : CP14RitualRequirement
{
    [DataField]
    public ProtoId<CP14MagicTypePrototype> MagicType = new();

    [DataField]
    public int? Min;

    [DataField]
    public int? Max;

    public override string? GetGuidebookRequirementDescription(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        var sb = new StringBuilder();

        if (!prototype.TryIndex(MagicType, out var indexedType))
            return null;

        sb.Append(Loc.GetString("cp14-ritual-required-orbs", ("name", Loc.GetString(indexedType.Name))) + " ");
        if (Min is not null && Max is not null)
            sb.Append(Loc.GetString("cp14-ritual-required-orbs-item-minmax", ("min", Min), ("max", Max))+ "\n");
        else if (Min is not null)
            sb.Append(Loc.GetString("cp14-ritual-required-orbs-item-min", ("min", Min))+ "\n");
        else if (Max is not null)
            sb.Append(Loc.GetString("cp14-ritual-required-orbs-item-min", ("max", Max))+ "\n");

        return sb.ToString();
    }

    public override bool Check(EntityManager entManager, Entity<CP14MagicRitualPhaseComponent> phaseEnt, float stability)
    {
        //if (phaseEnt.Comp.Ritual is null)
        //    return false;
//
        //if (!entManager.TryGetComponent<CP14MagicRitualComponent>(phaseEnt, out var ritualComp))
        //    return false;
//
        //var count = 0;
        //foreach (var orb in ritualComp.Orbs)
        //{
        //    foreach (var power in orb.Comp.Powers)
        //    {
        //        if (power.Key == MagicType)
        //            count += power.Value;
        //    }
        //}
//
        //if (Min is not null && Max is not null)
        //    return count >= Min && count <= Max;
        //if (Min is not null)
        //    return count >= Min;
        //if (Max is not null)
        //    return count <= Max;
//
        return false;
    }
}
