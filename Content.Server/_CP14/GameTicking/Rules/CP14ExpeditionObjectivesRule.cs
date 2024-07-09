using Content.Server._CP14.GameTicking.Rules.Components;
using Content.Server.Mind;

namespace Content.Server.GameTicking.Rules;

public sealed class CP14ExpeditionObjectivesRule : GameRuleSystem<CP14ExpeditionObjectivesRuleComponent>
{
    [Dependency] private readonly MindSystem _mind = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawning);
    }

    private void OnPlayerSpawning(PlayerSpawnCompleteEvent args)
    {
        var query = EntityQueryEnumerator<CP14ExpeditionObjectivesRuleComponent>();
        while (query.MoveNext(out var uid, out var expedition))
        {
            if (!_mind.TryGetMind(args.Player.UserId, out var mindId, out var mind))
            {
                Log.Error($"{ToPrettyString(args.Mob):player} was trying to get the expedition objectives by {ToPrettyString(uid):rule} but had no mind attached!");
                return;
            }

            foreach (var objective in expedition.Objectives)
            {
                _mind.TryAddObjective(mindId.Value, mind, objective);
            }
        }
    }
}
