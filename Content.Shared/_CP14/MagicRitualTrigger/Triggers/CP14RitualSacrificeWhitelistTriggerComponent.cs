namespace Content.Shared._CP14.MagicRitualTrigger.Triggers;

[RegisterComponent]
public sealed partial class CP14RitualSacrificeWhitelistTriggerComponent : Component
{
    [DataField]
    public HashSet<CP14SacrificeWhitelistTrigger> Triggers = new();
}
