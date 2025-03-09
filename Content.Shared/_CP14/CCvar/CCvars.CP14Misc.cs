using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    public static readonly CVarDef<int> CP14RoundEndMinutes =
        CVarDef.Create("cp14.round_end_minutes", 15, CVar.SERVERONLY);

    /// <summary>
    /// Automatically shuts down the server outside of the CBT plytime. Shitcoded enough, but it's temporary anyway
    /// </summary>
    public static readonly CVarDef<bool> CP14ClosedBetaTest =
        CVarDef.Create("cp14.closet_beta_test", false, CVar.SERVERONLY);
}
