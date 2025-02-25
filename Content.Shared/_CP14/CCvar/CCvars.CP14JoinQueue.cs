using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    public static readonly CVarDef<bool> QueueEnabled =
        CVarDef.Create("cp14.join_queue_enabled", true, CVar.SERVERONLY);
}
