using Content.Server._CP14.GameTicking.Rules.Components;
using Content.Server.Antag;
using Content.Server.GameTicking.Rules;
using Content.Shared.GameTicking.Components;
using Content.Shared.Objectives.Systems;
using Content.Shared.Players;

namespace Content.Server._CP14.GameTicking.Rules;

public sealed class CP14LurkerHuntRule : GameRuleSystem<CP14LurkerHuntRuleComponent>
{
    [Dependency] private readonly SharedObjectivesSystem _objectives = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private const string LurkerDefinitionKey = "Lurker";
    private const string HunterDefinitionKey = "Hunter";
    private const string VictimDefinitionKey = "Victim";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14LurkerHuntRuleComponent, AfterAntagEntitySelectedEvent>(AfterRoleSelected);
        SubscribeLocalEvent<CP14LurkerHuntRuleComponent, AntagSelectLocationEvent>(OnAntagSelectLocation);
        SubscribeLocalEvent<CP14LurkerHuntRuleComponent, AntagSelectEntityEvent>(OnAntagSelectEntity);
    }

    private void OnAntagSelectEntity(Entity<CP14LurkerHuntRuleComponent> ent, ref AntagSelectEntityEvent args)
    {
        if (args.Handled)
            return;

        if (args.Def is null)
            return;

        if (args.Def.Value.DefinitionKey == LurkerDefinitionKey)
        {
            args.Entity = Spawn(ent.Comp.LurkerProto);
        }
    }

    private void OnAntagSelectLocation(Entity<CP14LurkerHuntRuleComponent> ent, ref AntagSelectLocationEvent args)
    {
        if (args.Def is null)
            return;

        //Spawn lurker on random position on the map
        if (args.Def.Value.DefinitionKey == LurkerDefinitionKey && TryFindRandomTile(out _, out _, out _, out var coords))
        {
            args.Coordinates.Add(_transform.ToMapCoordinates(coords));
        }
    }

    private void AfterRoleSelected(Entity<CP14LurkerHuntRuleComponent> ent, ref AfterAntagEntitySelectedEvent args)
    {
        if (args.Session is null)
            return;

        var mind = args.Session.GetMind();

        if (mind is null)
            return;

        switch (args.Def.DefinitionKey)
        {
            case LurkerDefinitionKey:
                ent.Comp.Lurker = mind;
                break;
            case VictimDefinitionKey:
                ent.Comp.Victims.Add(mind.Value);
                break;
            case HunterDefinitionKey:
                ent.Comp.Hunters.Add(mind.Value);
                break;
        }
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
