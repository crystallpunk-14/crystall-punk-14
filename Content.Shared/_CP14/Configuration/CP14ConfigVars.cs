using Robust.Shared;
using Robust.Shared.Configuration;

namespace Content.Shared._CP14.Configuration;

[CVarDefs]
public sealed class CP14ConfigVars : CVars
{
    public static readonly CVarDef<bool>
        WaveShaderEnabled = CVarDef.Create("cp14_rendering.wave_shader_enabled", true, CVar.CLIENT | CVar.ARCHIVE);
}
