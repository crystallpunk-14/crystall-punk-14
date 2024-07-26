using Content.Shared.Actions;
using Content.Shared.EntityEffects;

namespace Content.Shared._CP14.Magic.Events.Actions;

public sealed partial class CP14EntityEffectActionEvent : EntityTargetActionEvent
{
    [DataField(required: true, serverOnly: true)]
    public List<EntityEffect> Effects = new();
}
