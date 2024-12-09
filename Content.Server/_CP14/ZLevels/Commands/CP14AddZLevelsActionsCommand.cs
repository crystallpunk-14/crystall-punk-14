using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server._CP14.ZLevels.Commands;

[AdminCommand(AdminFlags.VarEdit)]
public sealed class CP14AddZLevelsActionsCommand : LocalizedCommands
{
    private const string Name = "cp14-addZLevelActions";
    public override string Command => Name;
    public override string Description => "Adds actions to fast move between ZLevels";
    public override string Help => $"{Name}";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        
    }
}
