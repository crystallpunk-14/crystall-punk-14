using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    public static readonly CVarDef<string> SponsorsApiUrl =
        CVarDef.Create("sponsors.api_url", "http://localhost:8000/sponsors", CVar.SERVERONLY | CVar.CONFIDENTIAL);

    public static readonly CVarDef<string> SponsorsApiKey =
        CVarDef.Create("sponsors.api_key", "token", CVar.SERVERONLY | CVar.CONFIDENTIAL);

    public static readonly CVarDef<int> PriorityJoinTier =
        CVarDef.Create("sponsors.priorityJoinTier", 2, CVar.SERVERONLY | CVar.CONFIDENTIAL);
}
