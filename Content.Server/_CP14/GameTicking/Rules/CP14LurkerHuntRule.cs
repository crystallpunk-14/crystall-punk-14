using Content.Server._CP14.GameTicking.Rules.Components;
using Content.Server.GameTicking.Rules;
using Content.Shared.GameTicking.Components;

namespace Content.Server._CP14.GameTicking.Rules;

public sealed class CP14LurkerHuntRule : GameRuleSystem<CP14LurkerHuntRuleComponent>
{
    public override void Initialize()
    {
        base.Initialize();

    }


    protected override void Added(EntityUid uid, CP14LurkerHuntRuleComponent component, GameRuleComponent gameRule, GameRuleAddedEvent args)
    {



    }

    protected override void Started(EntityUid uid,
        CP14LurkerHuntRuleComponent component,
        GameRuleComponent gameRule,
        GameRuleStartedEvent args)
    {

    }
}
