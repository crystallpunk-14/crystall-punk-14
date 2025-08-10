using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared.CCVar;
using Robust.Shared.Configuration;
using Robust.Shared.Console;

namespace Content.Server._CP14.Administration.Commands;

[AdminCommand(AdminFlags.Server)]
public sealed class SuspiciousLevelCommand : LocalizedCommands
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    public override string Command => "cp14.suspicious-warning-level";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length == 0)
        {
            var level = _cfg.GetCVar(CCVars.SuspiciousAccountsWarningLevel);
            shell.WriteLine(Loc.GetString("cp14-suspicious-warning-level-command-current", ("level", level)));
        }

        if (args.Length > 1)
        {
            shell.WriteError(Loc.GetString("shell-need-between-arguments",("lower", 0), ("upper", 1)));
            return;
        }

        List<string> possibleLevels = ["disabled", "low", "medium", "high"];
        if (!possibleLevels.Contains(args[0].ToLower()))
        {
            shell.WriteError(Loc.GetString("cp14-suspicious-warning-level-command-error"));
            return;
        }

        _cfg.SetCVar(CCVars.SuspiciousAccountsWarningLevel, args[0]);
        shell.WriteLine(Loc.GetString("cp14-suspicious-warning-level-command-set", ("level", args[0])));
    }
}
