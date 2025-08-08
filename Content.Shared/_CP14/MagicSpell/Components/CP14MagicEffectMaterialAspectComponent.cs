using Content.Shared._CP14.Workbench;

namespace Content.Shared._CP14.MagicSpell.Components;

/// <summary>
/// Requires the caster to hold a specific resource in their hand, which will be spent to use the spell.
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedMagicSystem))]
public sealed partial class CP14MagicEffectMaterialAspectComponent : Component
{
    [DataField(required: true)]
    public CP14WorkbenchCraftRequirement? Requirement;
}
