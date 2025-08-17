using Content.Shared._CP14.Skill.Prototypes;
using Content.Shared.CCVar;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._CP14.Skill.Restrictions;

public sealed partial class TimeGate : CP14SkillRestriction
{
    [DataField(required: true)]
    public int Minutes = 1;

    public override bool Check(IEntityManager entManager, EntityUid target, CP14SkillPrototype skill)
    {
        var timing = IoCManager.Resolve<IGameTiming>();
        var cfg = IoCManager.Resolve<IConfigurationManager>();

        if (cfg.GetCVar(CCVars.CP14SkillTimers) == false)
            return true;

        return timing.CurTime >= TimeSpan.FromMinutes(Minutes);
    }

    public override string GetDescription(IEntityManager entManager, IPrototypeManager protoManager)
    {
        var timing = IoCManager.Resolve<IGameTiming>();
        var cfg = IoCManager.Resolve<IConfigurationManager>();

        var leftoverTime = TimeSpan.FromMinutes(Minutes) - timing.CurTime;
        leftoverTime = leftoverTime < TimeSpan.Zero ? TimeSpan.Zero : leftoverTime;

        if (cfg.GetCVar(CCVars.CP14SkillTimers) == false)
            return Loc.GetString("cp14-skill-req-timegate-disabled", ("minute", Minutes));

        return Loc.GetString("cp14-skill-req-timegate", ("minute", Minutes), ("left", Math.Ceiling(leftoverTime.TotalMinutes)));
    }
}
