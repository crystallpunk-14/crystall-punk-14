using Content.Server.Administration;
using Content.Shared._CP14.DayCycle;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server._CP14.DayCycle;

[AdminCommand(AdminFlags.VarEdit)]
public sealed class CP14SetTimeEntryCommand : LocalizedCommands
{
    private const string Name = "cp14-settimeentry";
    private const int ArgumentCount = 2;

    public override string Command => Name;
    public override string Description => "Sets a new entry at the specified index";
    public override string Help => $"{Name} <mapUid> <timeEntry>";

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

        if (!int.TryParse(args[1], out var timeEntry))
        {
            shell.WriteError(Loc.GetString("parse-int-fail", ("args", args[1])));
            return;
        }

        dayCycleSystem.SetTimeEntry((entity, dayCycle), timeEntry);
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        var entityManager = IoCManager.Resolve<EntityManager>();

        switch (args.Length)
        {
            case 1:
                return CompletionResult.FromOptions(CompletionHelper.Components<CP14DayCycleComponent>(args[0], entityManager));

            case 2:
                if (!NetEntity.TryParse(args[0], out var mapUid))
                    return CompletionResult.Empty;

                if (!entityManager.TryGetComponent<CP14DayCycleComponent>(entityManager.GetEntity(mapUid), out var component))
                    return CompletionResult.Empty;

                if (component.TimeEntries.Count - 1 < 0)
                    return CompletionResult.Empty;

                var indices = new string[component.TimeEntries.Count - 1];
                for (var i = 0; i < indices.Length; i++)
                {
                    indices[i] = i.ToString();
                }

                return CompletionResult.FromOptions(indices);
        }

        return CompletionResult.Empty;
    }
}
