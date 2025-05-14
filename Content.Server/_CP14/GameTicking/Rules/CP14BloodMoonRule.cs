using Content.Server._CP14.GameTicking.Rules.Components;
using Content.Server.GameTicking.Rules;
using Content.Shared.GameTicking.Components;

namespace Content.Server._CP14.GameTicking.Rules;

public sealed class CP14BloodMoonRule : GameRuleSystem<CP14CommonObjectivesRuleComponent>
{
    public override void Initialize()
    {
        base.Initialize();


    }

    protected override void Added(EntityUid uid,
        CP14CommonObjectivesRuleComponent component,
        GameRuleComponent gameRule,
        GameRuleAddedEvent args)
    {
        base.Added(uid, component, gameRule, args);

        
    }
}
