using Content.Server._CP14.Demiplane;
using Content.Server._CP14.GameTicking.Rules.Components;
using Content.Server._CP14.Procedural;
using Content.Server.GameTicking.Rules;
using Content.Server.Station.Components;

namespace Content.Server._CP14.GameTicking.Rules;

public sealed class CP14StationAdditionalModifierRule : GameRuleSystem<CP14StationAdditionalModifierRuleComponent>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14BeforeStationLocationGenerationEvent>(OnBeforeStationLocationGeneration);
        SubscribeLocalEvent<BecomesStationComponent, CP14LocationGeneratedEvent>(OnFinishMapGeneration);
    }

    private void OnFinishMapGeneration(Entity<BecomesStationComponent> ent, ref CP14LocationGeneratedEvent args)
    {
        var query = QueryAllRules();
        while (query.MoveNext(out var uid, out var rule, out var gameRule))
        {

        }
    }

    private void OnBeforeStationLocationGeneration(CP14BeforeStationLocationGenerationEvent ev)
    {
        var query = QueryAllRules();
        while (query.MoveNext(out var uid, out var rule, out var gameRule))
        {
            ev.AddModifiers(rule.Modifiers);
        }
    }
}
