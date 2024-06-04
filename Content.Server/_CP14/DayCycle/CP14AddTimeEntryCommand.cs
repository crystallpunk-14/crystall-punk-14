using Content.Server.Administration;
using Content.Shared._CP14.DayCycle;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server._CP14.DayCycle;

[AdminCommand(AdminFlags.VarEdit)]
public sealed class CP14AddTimeEntryCommand : LocalizedCommands
{
    private const string Name = "cp14-addtimeentry";
    private const int ArgumentCount = 4;

    public override string Command => Name;
    public override string Description => "Allows you to add a new time entry to the map list";
    public override string Help => $"{Name} <mapUid> <color> <duration> <isNight>";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != ArgumentCount)
        {
            shell.WriteError($"{Loc.GetString("shell-wrong-arguments-number")}\n{Help}");
            return;
        }

        if (!NetEntity.TryParse(args[0], out var netEntity))
        {
            shell.WriteError(Loc.GetString("shell-entity-uid-must-be-number"));
            return;
        }

        var entityManager = IoCManager.Resolve<EntityManager>();
        var dayCycleSystem = entityManager.System<CP14DayCycleSystem>();
        var entity = entityManager.GetEntity(netEntity);

        if (!entityManager.TryGetComponent<CP14DayCycleComponent>(entity, out var dayCycle))
        {
            shell.WriteError(Loc.GetString("shell-entity-with-uid-lacks-component", ("uid", entity), ("componentName", nameof(CP14DayCycleComponent))));
            return;
        }

        if (!Color.TryParse(args[1], out var color))
        {
            shell.WriteError(Loc.GetString("parse-color-fail", ("args", args[1])));
            return;
        }

        if (!float.TryParse(args[2], out var duration))
        {
            shell.WriteError(Loc.GetString("parse-float-fail", ("args", args[2])));
            return;
        }

        if (!bool.TryParse(args[3], out var isNight))
        {
            shell.WriteError(Loc.GetString("parse-bool-fail", ("args", args[3])));
            return;
        }

        var entry = new DayCycleEntry
        {
            Color = color,
            Duration = TimeSpan.FromSeconds(duration),
            IsNight = isNight
        };

        dayCycleSystem.AddTimeEntry((entity, dayCycle), entry);
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return args.Length switch
        {
            1 => CompletionResult.FromOptions(CompletionHelper.Components<CP14DayCycleComponent>(args[0])),
            4 => CompletionResult.FromOptions(CompletionHelper.Booleans),
            _ => CompletionResult.Empty,
        };
    }
}
