using System.Linq;
using Content.Server._CP14.GameTicking.Rules.Components;
using Content.Server._CP14.Objectives.Systems;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Shared._CP14.Vampire;
using Content.Shared._CP14.Vampire.Components;
using Content.Shared.GameTicking;
using Content.Shared.GameTicking.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.GameTicking.Rules;

public sealed class CP14VampireRuleSystem : GameRuleSystem<CP14VampireRuleComponent>
{
    [Dependency] private readonly CP14VampireObjectiveConditionsSystem _condition = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    protected override void AppendRoundEndText(EntityUid uid,
        CP14VampireRuleComponent component,
        GameRuleComponent gameRule,
        ref RoundEndTextAppendEvent args)
    {
        base.AppendRoundEndText(uid, component, gameRule, ref args);

        //Alive percentage
        var alivePercentage = _condition.CalculateAlivePlayersPercentage();

        var aliveFactions = new HashSet<ProtoId<CP14VampireFactionPrototype>>();

        var query = EntityQueryEnumerator<CP14VampireComponent, MobStateComponent>();
        while (query.MoveNext(out var vampireUid, out var vampire, out var mobState))
        {
            if (mobState.CurrentState != MobState.Alive)
                continue;

            if (vampire.Faction is null)
                continue;

            aliveFactions.Add(vampire.Faction.Value);
        }

        args.AddLine($"[head=2][color=#ab1b3d]{Loc.GetString("cp14-vampire-clans-battle")}[/color][/head]");

        if (alivePercentage > _condition.RequiredAlivePercentage)
        {
            if (aliveFactions.Count == 0)
            {
                //City win
                args.AddLine($"[head=3][color=#7d112b]{Loc.GetString("cp14-vampire-clans-battle-clan-city-win")}[/color][/head]");
                args.AddLine(Loc.GetString("cp14-vampire-clans-battle-clan-city-win-desc"));
            }

            if (aliveFactions.Count == 1)
            {
                var faction = aliveFactions.First();

                if (_proto.TryIndex(faction, out var indexedFaction))
                    args.AddLine($"[head=3][color=#7d112b]{Loc.GetString("cp14-vampire-clans-battle-clan-win", ("name", Loc.GetString(indexedFaction.Name)))}[/color][/head]");
                args.AddLine(Loc.GetString("cp14-vampire-clans-battle-clan-win-desc"));
            }

            if (aliveFactions.Count == 2)
            {
                var factions = aliveFactions.ToArray();

                if (_proto.TryIndex(factions[0], out var indexedFaction1) && _proto.TryIndex(factions[1], out var indexedFaction2))
                    args.AddLine($"[head=3][color=#7d112b]{Loc.GetString("cp14-vampire-clans-battle-clan-tie-2", ("name1", Loc.GetString(indexedFaction1.Name)), ("name2", Loc.GetString(indexedFaction2.Name)))}[/color][/head]");
                args.AddLine(Loc.GetString("cp14-vampire-clans-battle-clan-tie-2-desc"));
            }

            if (aliveFactions.Count == 3)
            {
                args.AddLine($"[head=3][color=#7d112b]{Loc.GetString("cp14-vampire-clans-battle-clan-tie-3")}[/color][/head]");
                args.AddLine(Loc.GetString("cp14-vampire-clans-battle-clan-tie-3-desc"));
            }
        }
        else
        {
            args.AddLine($"[head=3][color=#7d112b]{Loc.GetString("cp14-vampire-clans-battle-clan-lose")}[/color][/head]");
            args.AddLine(Loc.GetString("cp14-vampire-clans-battle-clan-lose-desc"));
        }

        args.AddLine(Loc.GetString("cp14-vampire-clans-battle-alive-people", ("percent", alivePercentage * 100)));
    }
}
