using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    public static readonly CVarDef<int> CP14RoundEndMinutes =
        CVarDef.Create("cp14_demiplane.round_end_minutes", 15, CVar.SERVERONLY);
}
