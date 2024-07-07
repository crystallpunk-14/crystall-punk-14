using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server._CP14.GameTicking.Rules.Components;
using Content.Server._CP14.StationGamePresets;
using Content.Server.Administration.Logs;
using Content.Server.GameTicking.Presets;
using Content.Server.Station.Systems;
using Content.Shared.Database;
using Content.Shared.GameTicking.Components;
using Content.Shared.Random;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Server.GameTicking.Rules;

public sealed class CP14MapSpecificRuleSystem : GameRuleSystem<CP14MapSpecificRuleComponent>
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IComponentFactory _compFact = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;

    private string _ruleCompName = default!;
    public override void Initialize()
    {
        base.Initialize();
        _ruleCompName = _compFact.GetComponentName(typeof(GameRuleComponent));
    }

    protected override void Added(EntityUid uid, CP14MapSpecificRuleComponent component, GameRuleComponent gameRule, GameRuleAddedEvent args)
    {
        base.Added(uid, component, gameRule, args);

        var stations = _station.GetStations();
        foreach (var station in stations)
        {
            if (!TryComp<CP14StationGamePresetsComponent>(station, out var gamePresets))
                continue;

            if (!TryPickPreset(gamePresets.WeightPresets, out var preset))
            {
                Log.Error($"{ToPrettyString(uid)} failed to pick any preset. Removing rule.");
                Del(uid);
                return;
            }

            Log.Info($"Selected {preset.ID} as the secret preset.");
            _adminLogger.Add(LogType.EventStarted, $"Selected {preset.ID} as the secret preset.");

            foreach (var rule in preset.Rules)
            {
                EntityUid ruleEnt;

                // if we're pre-round (i.e. will only be added)
                // then just add rules. if we're added in the middle of the round (or at any other point really)
                // then we want to start them as well
                if (GameTicker.RunLevel <= GameRunLevel.InRound)
                    ruleEnt = GameTicker.AddGameRule(rule);
                else
                    GameTicker.StartGameRule(rule, out ruleEnt);

                component.AdditionalGameRules.Add(ruleEnt);
            }
        }
    }

    protected override void Ended(EntityUid uid, CP14MapSpecificRuleComponent component, GameRuleComponent gameRule, GameRuleEndedEvent args)
    {
        base.Ended(uid, component, gameRule, args);

        foreach (var rule in component.AdditionalGameRules)
        {
            GameTicker.EndGameRule(rule);
        }
    }

    private bool TryPickPreset(ProtoId<WeightedRandomPrototype> weights,
        [NotNullWhen(true)] out GamePresetPrototype? preset)
    {

        var options = _proto.Index(weights).Weights.ShallowClone();
        var players = GameTicker.ReadyPlayerCount();

        GamePresetPrototype? selectedPreset = null;
        var sum = options.Values.Sum();
        while (options.Count > 0)
        {
            var accumulated = 0f;
            var rand = _random.NextFloat(sum);
            foreach (var (key, weight) in options)
            {
                accumulated += weight;
                if (accumulated < rand)
                    continue;

                if (!_proto.TryIndex(key, out selectedPreset))
                    Log.Error($"Invalid preset {selectedPreset} in secret rule weights: {weights}");

                options.Remove(key);
                sum -= weight;
                break;
            }

            if (CanPick(selectedPreset, players))
            {
                preset = selectedPreset;
                return true;
            }

            if (selectedPreset != null)
                Log.Info($"Excluding {selectedPreset.ID} from secret preset selection.");
        }

        preset = null;
        return false;
    }

    /// <summary>
    /// Can the given preset be picked, taking into account the currently available player count?
    /// </summary>
    private bool CanPick([NotNullWhen(true)] GamePresetPrototype? selected, int players)
    {
        if (selected == null)
            return false;

        foreach (var ruleId in selected.Rules)
        {
            if (!_proto.TryIndex(ruleId, out EntityPrototype? rule)
                || !rule.TryGetComponent(_ruleCompName, out GameRuleComponent? ruleComp))
            {
                Log.Error($"Encountered invalid rule {ruleId} in preset {selected.ID}");
                return false;
            }

            if (ruleComp.MinPlayers > players && ruleComp.CancelPresetOnTooFewPlayers)
                return false;
        }

        return true;
    }
}
