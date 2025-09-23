using Content.Server._CP14.GameTicking.Rules.Components;
using Content.Server._CP14.Procedural;
using Content.Server.GameTicking.Rules;

namespace Content.Server._CP14.GameTicking.Rules;

public sealed class CP14StationAdditionalModifierRule : GameRuleSystem<CP14StationAdditionalModifierRuleComponent>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14BeforeStationLocationGenerationEvent>(OnBeforeStationLocationGeneration);
    }

    private void OnBeforeStationLocationGeneration(CP14BeforeStationLocationGenerationEvent ev)
    {
        var query = QueryAllRules();
        while (query.MoveNext(out var uid, out var huntRule, out var gameRule))
        {
            ev.AddModifiers(huntRule.Modifiers);
        }
    }
}
