namespace Content.Shared._CP14.MagicRitualTrigger.Triggers;

[RegisterComponent]
public sealed partial class CP14RitualSacrificeSpeciesTriggerComponent : Component
{
    [DataField]
    public HashSet<CP14SacrificeSpeciesTrigger> Triggers = new();
}
