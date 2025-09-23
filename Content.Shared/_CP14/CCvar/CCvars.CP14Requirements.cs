using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    /// <summary>
    ///     If roundstart species should be restricted based on time.
    /// </summary>
    public static readonly CVarDef<bool>
        CP14RoundstartSpeciesTimers = CVarDef.Create("game.species_timers", true, CVar.SERVER | CVar.REPLICATED);
}
