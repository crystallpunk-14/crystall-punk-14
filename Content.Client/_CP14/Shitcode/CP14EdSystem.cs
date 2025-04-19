using Content.Shared.CCVar;
using Robust.Shared;
using Robust.Shared.Configuration;

namespace Content.Client._CP14.Shitcode;

/// <summary>
/// Эта система - сборник разного мелкого барахла, который слишком мелкий чтобы иметь свои собственные системы. В идеале в будущем разнести по отдельным файлам.
/// </summary>
public sealed class CP14EdSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    public override void Initialize()
    {
        _cfg.SetCVar(CVars.EntitiesCategoryFilter, "ForkFiltered");

        _cfg.OnValueChanged(CCVars.Language, OnLanguageChange, true);
        
        _cfg.SetCVar(CVars.LocCultureName, _cfg.GetCVar(CCVars.Language));
    }

    private void OnLanguageChange(string obj)
    {
        _cfg.SetCVar(CVars.LocCultureName, obj);
    }
}
