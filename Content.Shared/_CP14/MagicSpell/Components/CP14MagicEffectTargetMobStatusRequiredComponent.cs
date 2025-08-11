using Content.Shared.Mobs;
using Robust.Shared.GameStates;

namespace Content.Shared._CP14.MagicSpell.Components;

/// <summary>
/// Allows you to limit the use of a spell based on the target's alive/dead status
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class CP14MagicEffectTargetMobStatusRequiredComponent : Component
{
    [DataField]
    public LocId Popup = "cp14-magic-spell-target-alive";

    [DataField]
    public HashSet<MobState> AllowedStates = new() { MobState.Alive };
}
